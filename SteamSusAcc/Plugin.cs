using Exiled.Events.EventArgs.Player;
using Log = Exiled.API.Features.Log;
using Exiled.API.Features;
using System.Net.Http;
using System;
using LiteDB;
using static SteamSusAcc.Data;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SteamSusAcc
{
    public class Plugin : Plugin<Config>
    {
        public override string Prefix => "SteamAPI";
        public override string Name => "SteamAPI";
        public override string Author => "angelseraphim.";
        public override Version Version => new Version(1, 5, 0);

        public static Plugin plugin;
        public static Webhook webhook;
        public LiteDatabase db { get; set; }

        private string apiKey;

        public override void OnEnabled()
        {
            plugin = this;
            webhook = new Webhook();
            if (Config.SaveToData)
                db = new LiteDatabase($"{GetParentDirectory(2)}/SteamAPI{Server.Port}.db");
            if (!Config.SteamDevKey.IsEmpty())
            {
                apiKey = Config.SteamDevKey;
                if (Config.DiscordWebHook.IsEmpty())
                    Log.Warn("Webhook URL not found.");
                Exiled.Events.Handlers.Player.Verified += OnVerified;
            }
            else
            {
                Log.Error("Steam API key not found! https://steamcommunity.com/dev/apikey");
            }
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            plugin = null;
            webhook = null;
            db.Dispose();
            db = null;
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            base.OnDisabled();
        }

        private async void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNorthwoodStaff || ev.Player.UserId.Contains("northwood"))
            {
                if (Config.DisconnectNorthwoods) 
                    ev.Player.Disconnect(Config.DisconnectNorthwoodsReason);
                return;
            }
            if (ev.Player.UserId.Contains("discord"))
            {
                if (Config.DisconnectDiscordPlayers)
                    ev.Player.Disconnect(Config.DisconnectDiscordPlayersReason);
                return;
            }
            Log.Debug("Checking...");
            if (Config.SaveToData && Extensions.GetPlayer(ev.Player.UserId, out PlayerInfo info))
            {
                Log.Debug("Player is in db");
                return;
            }
            Log.Debug("Checking... 2");

            string x = ev.Player.UserId;
            int x1 = x.Length - 6;
            x = x.Remove(x1);
            string apiUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={x}";

            try
            {
                Log.Debug($"Trying...");
                using (HttpClient client = new HttpClient())
                {
                    Log.Debug($"Getting info");
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(jsonResponse);

                    var player = json["response"]["players"][0];
                    int AccPrivacy = (int)player["communityvisibilitystate"];
                    Log.Debug($"Player acc privacy: {AccPrivacy}");

                    if (AccPrivacy == 1 && Config.CheckAccPrivacy)
                    {
                        Log.Debug($"Player acc is pravate {AccPrivacy}");
                        Config.CheckPrivacy.Apply(ev.Player, true);
                        return;
                    }
                    DateTime registrationDate = DateTimeOffset.FromUnixTimeSeconds((long)player["timecreated"]).DateTime;
                    Log.Debug($"Getting registor date");
                    if (DateTime.Now - registrationDate < TimeSpan.FromDays(Config.CheckAge.MinDays))
                    {
                        Config.CheckAge.Apply(ev.Player, true);
                        Log.Debug($"Player acc too young {DateTime.Now - registrationDate}");
                        return;
                    }
                    if (Config.CheckBans)
                    {
                        string playerBanUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={apiKey}&steamids={x}";
                        HttpResponseMessage banResponse = await client.GetAsync(playerBanUrl);
                        banResponse.EnsureSuccessStatusCode();
                        string banJsonResponse = await banResponse.Content.ReadAsStringAsync();
                        JObject banJson = JObject.Parse(banJsonResponse);
                        int numberOfVacBans = (int)banJson["players"][0]["NumberOfVACBans"];
                        int numberOfGameBans = (int)banJson["players"][0]["NumberOfGameBans"];
                        Log.Debug($"Getting bans count");

                        if (numberOfVacBans >= Config.CheckBan.MinVacBans || numberOfGameBans >= Config.CheckBan.MinGameBans || (numberOfVacBans + numberOfGameBans >= Config.CheckBan.MinTotalBans))
                        {
                            webhook.Send(Config.DiscordWebHook, Config.CheckBan.WebhookText.Replace("%playerinfo%", GetPlayerInfo(ev.Player)).Replace("%vacbans%", numberOfVacBans.ToString()).Replace("%gamebans%", numberOfGameBans.ToString()));
                            if (Config.CheckBan.Disconnect)
                            {
                                Config.CheckBan.Apply(ev.Player, true);
                                return;
                            }
                        }
                    }
                    if (Config.CheckGameTime.MinHours > 0)
                    {
                        string scpSlGameId = "700330";
                        string gameTimeUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={x}&format=json";
                        HttpResponseMessage gameTimeResponse = await client.GetAsync(gameTimeUrl);
                        gameTimeResponse.EnsureSuccessStatusCode();
                        string gameTimeJsonResponse = await gameTimeResponse.Content.ReadAsStringAsync();
                        JObject gameTimeJson = JObject.Parse(gameTimeJsonResponse);
                        Log.Debug($"Getting games info");

                        if (gameTimeJson["response"] == null || gameTimeJson["response"]["games"] == null)
                        {
                            Log.Debug($"Player acc is pravate {AccPrivacy}");
                            Config.CheckGameTime.Apply(ev.Player, false);
                            return;
                        }
                        var games = gameTimeJson["response"]["games"];
                        bool hasScpSl = false;
                        int scpSlPlaytimeMinutes = 0;
                        foreach (var game in games)
                        {
                            if ((string)game["appid"] == scpSlGameId)
                            {
                                hasScpSl = true;
                                scpSlPlaytimeMinutes = (int)game["playtime_forever"];
                                break;
                            }
                        }
                        if (hasScpSl && scpSlPlaytimeMinutes < (Config.CheckGameTime.MinHours * 60))
                        {
                            Log.Debug($"Player acc The player has too few hours in SCP:SL {scpSlPlaytimeMinutes}");
                            Config.CheckGameTime.Apply(ev.Player, true);
                            return;
                        }
                    }
                    AddToData(ev.Player.UserId);
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"HTTP error: {ex.Message}");
                if (Config.FailDisconnect)
                    ev.Player.Disconnect(Config.FailDisconnectReason.Replace("%error%", ex.Message));
            }
        }
        public string GetPlayerInfo(Player player)
        {
            return $"{player.Nickname} ({player.UserId}) [{player.IPAddress}]";
        }
        public void AddToData(string Id)
        {
            if (Config.SaveToData)
                Extensions.InsertPlayer(Id);
        }
        private string GetParentDirectory(int levels)
        {
            string parentPath = Path.GetDirectoryName(ConfigPath);
            for (int i = 0; i < levels; i++)
            {
                parentPath = Directory.GetParent(parentPath)?.FullName;
                if (parentPath == null)
                {
                    throw new InvalidOperationException("Невозможно подняться выше корневой директории.");
                }
            }
            return parentPath;
        }
    }
}   