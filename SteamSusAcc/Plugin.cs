using Exiled.Events.EventArgs.Player;
using Log = Exiled.API.Features.Log;
using Exiled.API.Features;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System;
using LiteDB;
using static SteamSusAcc.Data;
using Newtonsoft.Json.Linq;

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

        static async Task SendDiscordWebhook(string webhookUrl, string message)
        {
            using (var httpClient = new HttpClient())
            {
                var payload = new
                {
                    content = message
                };

                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, httpContent);

                if (response.IsSuccessStatusCode)
                    Log.Info("Message sent successfully!");
                else
                    Log.Error($"Failed to send message. Status code: {response.StatusCode}");
            }
        }
        public override void OnEnabled()
        {
            plugin = this;
            db = new LiteDatabase("../.config/EXILED/Configs/SteamAPI.db");
            if (!Config.SteamDevKey.IsEmpty())
            {
                apiKey = Config.SteamDevKey;
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
                Exiled.Events.Handlers.Player.Verified += OnVerified;
            }
            else
            {
                Log.Error("Steam API key not found! https://steamcommunity.com/dev/apikey");
            }
            base.OnDisabled();
        }

        private async void OnVerified(VerifiedEventArgs ev)
        {
            string x = ev.Player.UserId;
            int x1 = x.Length - 6;
            x = x.Remove(x1);
            string apiUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={x}";

            if (ev.Player.IsNorthwoodStaff || ev.Player.UserId.Contains("northwood"))
            {
                return;
            }
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

                    if ((int)player["communityvisibilitystate"] == 1 && Config.AccPrivacy)
                    {
                        ev.Player.Disconnect(Config.AccPrivacyKickReason);
                        return;
                    }
                    DateTime registrationDate = DateTimeOffset.FromUnixTimeSeconds((long)player["timecreated"]).DateTime;
                    if (DateTime.Now - registrationDate < TimeSpan.FromDays(Config.MinAccAge))
                    {
                        ev.Player.Disconnect(Config.MinAccAgeKickReason);
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

                        if (numberOfVacBans >= Config.MinVacBan || numberOfGameBans >= Config.MinGameBan || (numberOfVacBans + numberOfGameBans >= Config.MinTotalBans))
                        {
                            ev.Player.Disconnect(Config.MinBanKickReason);
                            return;
                        }
                    }
                    if (Config.MinHours != 0)
                    {
                        string scpSlGameId = "700330";
                        string gameTimeUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={x}&format=json";
                        HttpResponseMessage gameTimeResponse = await client.GetAsync(gameTimeUrl);
                        gameTimeResponse.EnsureSuccessStatusCode();
                        string gameTimeJsonResponse = await gameTimeResponse.Content.ReadAsStringAsync();
                        JObject gameTimeJson = JObject.Parse(gameTimeJsonResponse);

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
            }
        }
        public void AddToData(string Id) => Extensions.InsertPlayer(Id);
    }
}   