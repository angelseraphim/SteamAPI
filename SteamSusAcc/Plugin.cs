using Exiled.Events.EventArgs.Player;
using Log = Exiled.API.Features.Log;
using Exiled.API.Features;
using System.Net.Http;
using System;
using LiteDB;
using static SteamSusAcc.DataBase.Data;
using Extensions = SteamSusAcc.DataBase.Extensions;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SteamSusAcc
{
    public class Plugin : Plugin<Config>
    {
        public override string Prefix => "SteamAPI";
        public override string Name => "SteamAPI";
        public override string Author => "angelseraphim.";
        public override Version Version => new Version(1, 5, 3);

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
            apiKey = Config.SteamDevKey;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
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
            if (Config.SteamDevKey.IsEmpty())
            {
                Log.Error("Steam API key is empty");
                return;
            }

            if (HandleSpecialPlayers(ev)) 
                return;

            Log.Debug("Checking...");

            if (Config.SaveToData && Extensions.GetPlayer(ev.Player.UserId, out PlayerInfo info))
            {
                Log.Debug("Player is in DB");
                return;
            }

            Log.Debug("Checking... 2");
            string steamId = GetSteamId(ev.Player.UserId);

            try
            {
                var playerData = await FetchPlayerData(steamId);
                if (playerData == null) 
                    return;

                if (HandleAccountPrivacy(ev, playerData)) 
                    return;

                if (HandleAccountAge(ev, playerData)) 
                    return;

                if (await HandleBans(ev, steamId)) 
                    return;

                if (await HandleGameTime(ev, steamId)) 
                    return;

                AddToData(ev.Player.UserId);
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"HTTP error: {ex.Message}");
                if (Config.FailDisconnect)
                    ev.Player.Disconnect(Config.FailDisconnectReason.Replace("%error%", ex.Message));
            }
        }

        private bool HandleSpecialPlayers(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNorthwoodStaff || ev.Player.UserId.Contains("northwood"))
            {
                if (Config.DisconnectNorthwoods)
                    ev.Player.Disconnect(Config.DisconnectNorthwoodsReason);
                return true;
            }

            if (ev.Player.UserId.Contains("discord"))
            {
                if (Config.DisconnectDiscordPlayers)
                    ev.Player.Disconnect(Config.DisconnectDiscordPlayersReason);
                return true;
            }

            return false;
        }

        private string GetSteamId(string userId)
        {
            return userId.Remove(userId.Length - 6);
        }

        private async Task<JObject> FetchPlayerData(string steamId)
        {
            string apiUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={Config.SteamDevKey}&steamids={steamId}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JObject.Parse(jsonResponse)["response"]["players"]?[0] as JObject;
            }
        }

        private bool HandleAccountPrivacy(VerifiedEventArgs ev, JObject playerData)
        {
            int privacyState = (int)playerData["communityvisibilitystate"];
            Log.Debug($"Player account privacy: {privacyState}");

            if (privacyState == 1 && Config.CheckAccPrivacy)
            {
                Log.Debug($"Player account is private: {privacyState}");
                Config.CheckPrivacy.Apply(ev.Player, true);
                return true;
            }

            return false;
        }

        private bool HandleAccountAge(VerifiedEventArgs ev, JObject playerData)
        {
            DateTime registrationDate = DateTimeOffset.FromUnixTimeSeconds((long)playerData["timecreated"]).DateTime;
            Log.Debug($"Player registration date: {registrationDate}");

            if (DateTime.Now - registrationDate < TimeSpan.FromDays(Config.CheckAge.MinDays))
            {
                Config.CheckAge.Apply(ev.Player, true);
                Log.Debug($"Player account is too young: {DateTime.Now - registrationDate}");
                return true;
            }

            return false;
        }

        private async Task<bool> HandleBans(VerifiedEventArgs ev, string steamId)
        {
            if (!Config.CheckBans) 
                return false;

            string banUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={Config.SteamDevKey}&steamids={steamId}";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage banResponse = await client.GetAsync(banUrl);
                banResponse.EnsureSuccessStatusCode();
                string banJson = await banResponse.Content.ReadAsStringAsync();
                JObject banData = JObject.Parse(banJson)["players"]?[0] as JObject;

                int vacBans = (int)banData["NumberOfVACBans"];
                int gameBans = (int)banData["NumberOfGameBans"];

                Log.Debug($"Player VAC bans: {vacBans}, Game bans: {gameBans}");

                if (vacBans >= Config.CheckBan.MinVacBans || gameBans >= Config.CheckBan.MinGameBans ||
                    (vacBans + gameBans >= Config.CheckBan.MinTotalBans))
                {
                    webhook.Send(Config.DiscordWebHook, Config.CheckBan.WebhookText
                        .Replace("%playerinfo%", GetPlayerInfo(ev.Player))
                        .Replace("%vacbans%", vacBans.ToString())
                        .Replace("%gamebans%", gameBans.ToString()));

                    if (Config.CheckBan.Disconnect)
                    {
                        Config.CheckBan.Apply(ev.Player, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> HandleGameTime(VerifiedEventArgs ev, string steamId)
        {
            if (Config.CheckGameTime.MinHours <= 0) 
                return false;

            string gameTimeUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={Config.SteamDevKey}&steamid={steamId}&format=json";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(gameTimeUrl);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject gamesData = JObject.Parse(jsonResponse)["response"] as JObject;

                if (gamesData == null || gamesData["games"] == null)
                {
                    Log.Debug("Player account is private or has no games");
                    Config.CheckGameTime.Apply(ev.Player, false);
                    return true;
                }

                var games = gamesData["games"];
                int scpPlaytime = games
                    .Where(g => (string)g["appid"] == "700330")
                    .Select(g => (int)g["playtime_forever"])
                    .FirstOrDefault();

                if (scpPlaytime < Config.CheckGameTime.MinHours * 60)
                {
                    Log.Debug($"Player has too few hours in SCP:SL: {scpPlaytime}");
                    Config.CheckGameTime.Apply(ev.Player, true);
                    return true;
                }
            }

            return false;
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
                    throw new InvalidOperationException("It is impossible to go higher than the root directory.");
                }
            }
            return parentPath;
        }
    }
}   