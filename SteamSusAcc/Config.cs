using Exiled.API.Interfaces;
using SteamSusAcc.Constructor;
using System.ComponentModel;

namespace SteamSusAcc
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string DiscordWebHook { get; set; } = "";
        [Description("Steam API Dev key. https://steamcommunity.com/dev/apikey Follow the link and write \"LocalHost\"")]
        public string SteamDevKey { get; set; } = "";
        [Description("Save players to the database? Saving to the database will help to delay verification if the player has already logged into the server before")]
        public bool SaveToData { get; set; } = true;
        [Description("Disconnect players with DiscordUserID@discord ID?")]
        public bool DisconnectDiscordPlayers { get; set; } = false;
        public string DisconnectDiscordPlayersReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nCould not find your SteamID";
        [Description("Disconnect players with DiscordUserID@northwood ID?")]
        public bool DisconnectNorthwoods { get; set; } = false;
        public string DisconnectNorthwoodsReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nCould not find your SteamID";
        [Description("Disconnect a player if Steam verification fails?")]
        public bool FailDisconnect { get; set; } = false;
        public string FailDisconnectReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nHttp request error. Error: %error%";
        [Description("Check account privacy (If the check is disabled, other checks will also stop working.)")]
        public bool CheckAccPrivacy { get; set;} = true;
        public Privacy CheckPrivacy { get; set; } = new Privacy("<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too sus", "", "%playerinfo% was disconnected from the server due to privacy settings");

        [Description("Check how many hours a player has been playing SCP:SL (In hours. Change to -1 to disable")]
        public GameTime CheckGameTime { get; set; } = new GameTime(5, "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYou don't have enough hours to play on the server", "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nInformation about games is hidden, please make the information public", "%playerinfo% was disconnected from the server due to too little time in SCP:SL");

        [Description("Check account age (In days. Set to -1 to disable)")]
        public Age CheckAge { get; set; } = new Age(7, "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too young", "", "%playerinfo% was disconnected from the server due to the account being too young");

        [Description("Check account bans. (VAС bans and Game bans)")]
        public bool CheckBans { get; set; } = false;
        public Bans CheckBan { get; set; } = new Bans(false, 2, 2, 3, "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc have too many bans", "", "%playerinfo% Player has too many bans\nVac bans count: %vacbans%\n Game bans count: %gamebans%");
    }
}
