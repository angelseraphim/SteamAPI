using Exiled.Events.EventArgs.Player;
using Log = Exiled.API.Features.Log;
using Exiled.API.Features;
using System.Net.Http;
using System.Text;
using System;
using LiteDB;
using static SteamSusAcc.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Exiled.API.Interfaces;
using System.IO;

namespace SteamSusAcc
{
    public class Plugin : Plugin<Config>
    {
        public override string Prefix => "SteamAPI";
        public override string Name => "SteamAPI";
        public override string Author => "angelseraphim.";
        public override Version Version => base.Version;
        public override Version RequiredExiledVersion => new Version(8, 9, 7);

        public static Plugin plugin;
        string apiKey;
        public LiteDatabase db { get; set; }

        public static void SendDiscordWebhook(string WebhookURL, string Text, string Title = null, string WebhookText = null, string AvatarURL = null, string ImageURL = null, int Color = 0xff0000)
        {
            try
            {
                var message = new
                {
                    content = Text,
                    avatar_url = AvatarURL,
                    embeds = new[]
    {
                    new
                    {
                        color = Color,
                        title = Title,
                        description = WebhookText,
                        image = new { url = ImageURL }
                    }
                }
                };

                var client = new HttpClient();
                var json = JsonConvert.SerializeObject(message);

                client.PostAsync(WebhookURL, new StringContent(json, Encoding.UTF8, "application/json"));
            }
            catch
            {
                Log.Error("Webhook is wrong. Please check your configuration");
            }
        }
        public override void OnEnabled()
        {
            plugin = this;
            db = new LiteDatabase($"{GetParentDirectory(2)}/SteamAPI.db");
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
            db.Dispose();
            db = null;
            if (!Config.SteamDevKey.IsEmpty())
            {
                if (typeof(IPlugin<>).Assembly.GetName().Version >= new Version(8, 9, 7))
                {
                    apiKey = Config.SteamDevKey;
                    if (Config.DiscordWebHook.IsEmpty())
                        Log.Warn("Webhook URL not found.");
                    Exiled.Events.Handlers.Player.Verified -= OnVerified;
                }
                else
                {
                    Log.Error("Incorrect version of Exiled. Please install version 8.9.7 or higher");
                }
            }
            else
            {
                Log.Error("Steam API key not found! https://steamcommunity.com/dev/apikey");
            }
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

            string x = ev.Player.UserId;
            int x1 = x.Length - 6;
            x = x.Remove(x1);
            string apiUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={x}";

            if (Extensions.GetPlayer(ev.Player.UserId, out PlayerInfo info))
            {
                Log.Debug("player is in db");
                return;
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(jsonResponse);

                    var player = json["response"]["players"][0];
                    int AccPrivacy = (int)player["communityvisibilitystate"];
                    DateTime registrationDate = DateTimeOffset.FromUnixTimeSeconds((long)player["timecreated"]).DateTime;

                    string playerBanUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={apiKey}&steamids={x}";
                    HttpResponseMessage banResponse = await client.GetAsync(playerBanUrl);
                    banResponse.EnsureSuccessStatusCode();
                    string banJsonResponse = await banResponse.Content.ReadAsStringAsync();
                    JObject banJson = JObject.Parse(banJsonResponse);
                    int numberOfVacBans = (int)banJson["players"][0]["NumberOfVACBans"];
                    int numberOfGameBans = (int)banJson["players"][0]["NumberOfGameBans"];

                    string scpSlGameId = "700330";
                    string gameTimeUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={x}&format=json";
                    HttpResponseMessage gameTimeResponse = await client.GetAsync(gameTimeUrl);
                    gameTimeResponse.EnsureSuccessStatusCode();
                    string gameTimeJsonResponse = await gameTimeResponse.Content.ReadAsStringAsync();
                    JObject gameTimeJson = JObject.Parse(gameTimeJsonResponse);

                    if (AccPrivacy == 1 && Config.AccPrivacy)
                    {
                        SendDiscordWebhook(Config.DiscordWebHook, Config.AccPrivacyDiscord.Replace(":playerinfo:", GetPlayerInfo(ev.Player)));
                        ev.Player.Disconnect(Config.AccPrivacyKickReason);
                        return;
                    }
                    if (DateTime.Now - registrationDate < TimeSpan.FromDays(Config.MinAccAge))
                    {
                        SendDiscordWebhook(Config.DiscordWebHook, Config.MinAccAgeDiscord.Replace(":playerinfo:", GetPlayerInfo(ev.Player)));
                        ev.Player.Disconnect(Config.MinAccAgeKickReason);
                        return;
                    }
                    if (Config.CheckBans)
                    {
                        if (numberOfVacBans >= Config.MinVacBan || numberOfGameBans >= Config.MinGameBan || (numberOfVacBans + numberOfGameBans >= Config.MinTotalBans))
                        {
                            SendDiscordWebhook(Config.DiscordWebHook, Config.CheckBansDiscord.Replace(":playerinfo:", GetPlayerInfo(ev.Player)).Replace(":vacbans:", numberOfVacBans.ToString()).Replace(":gamebans:", numberOfGameBans.ToString()));
                            if (Config.CheckBansKick)
                            {
                                ev.Player.Disconnect(Config.MinBanKickReason);
                                return;
                            }
                        }
                    }

                    if (Config.MinHours != 0)
                    {
                        if (gameTimeJson["response"] == null || gameTimeJson["response"]["games"] == null)
                        {
                            ev.Player.Disconnect(Config.MinHoursKickReason2);
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
                        if (hasScpSl && scpSlPlaytimeMinutes < (Config.MinHours * 60))
                        {
                            SendDiscordWebhook(Config.DiscordWebHook, Config.MinHoursDiscord.Replace(":playerinfo:", GetPlayerInfo(ev.Player)));
                            ev.Player.Disconnect(Config.MinHoursKickReason);
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
                    ev.Player.Disconnect(Config.FailDisconnectReason.Replace(":error:", ex.Message));
            }
        }
        private string GetPlayerInfo(Player player)
        {
            return $"{player.Nickname} ({player.UserId}) [{player.IPAddress}]";
        }
        public void AddToData(string Id) => Extensions.InsertPlayer(Id);
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