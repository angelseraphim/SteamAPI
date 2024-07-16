using Exiled.API.Interfaces;
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
        [Description("Disconnect players with DiscordUserID@discord ID?")]
        public bool DisconnectDiscordPlayers { get; set; } = false;
        public string DisconnectDiscordPlayersReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nCould not find your SteamID";
        [Description("Disconnect players with DiscordUserID@northwood ID?")]
        public bool DisconnectNorthwoods { get; set; } = false;
        public string DisconnectNorthwoodsReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nCould not find your SteamID";

        [Description("Check how many hours a player has been playing SCP:SL (In hours. Change to 0 to disable")]
        public int MinHours { get; set; } = 0;
        public string MinHoursDiscord { get; set; } = ":playerinfo: was disconnected from the server due to too little time in SCP:SL ";
        public string MinHoursKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYou don't have enough hours to play on the server";
        public string MinHoursKickReason2 { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nInformation about games is hidden, please make the information public";

        [Description("Check account privacy")]
        public bool AccPrivacy { get; set; } = true;
        public string AccPrivacyDiscord { get; set; } = ":playerinfo: was disconnected from the server due to privacy settings";
        public string AccPrivacyKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too sus";
        [Description("Check account age (In days. Set to 0 to disable)")]
        public int MinAccAge { get; set; } = 7;
        public string MinAccAgeDiscord { get; set; } = ":playerinfo: was disconnected from the server due to the account being too young";
        public string MinAccAgeKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too young";
        [Description("Check account bans. (VAС bans and Game bans)")]
        public bool CheckBans { get; set; } = false;
        public bool CheckBansKick { get; set; } = true;
        public string CheckBansDiscord { get; set; } = ":playerinfo: Player has too many bans\nVac bans count: :vacbans:\n Game bans count: :gamebans:";
        public int MinTotalBans { get; set; } = 3;
        public int MinVacBan { get; set; } = 2;
        public int MinGameBan { get; set; } = 2;
        public string MinBanKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc have too many bans";

    }
}
