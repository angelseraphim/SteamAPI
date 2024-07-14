using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SteamSusAcc
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        [Description("Steam API Dev key. https://steamcommunity.com/dev/apikey Follow the link and write \"LocalHost\"")]
        public string SteamDevKey { get; set; } = "";
        [Description("Check how many hours a player has been playing SCP:SL (In hours. Change to 0 to disable")]
        public int MinHours { get; set; } = 0;
        public string MinHoursKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYou don't have enough hours to play on the server";
        public string MinHoursKickReason2 { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nInformation about games is hidden, please make the information public";

        [Description("Check account privacy")]
        public bool AccPrivacy { get; set; } = true;
        public string AccPrivacyKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too sus";
        [Description("Check account age (In days. Set to 0 to disable)")]
        public int MinAccAge { get; set; } = 7;
        public string MinAccAgeKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc too young";
        [Description("Check account bans. (VAС bans and Game bans)")]
        public bool CheckBans { get; set; } = false;
        public int MinTotalBans { get; set; } = 3;
        public int MinVacBan { get; set; } = 2;
        public int MinGameBan { get; set; } = 2;
        public string MinBanKickReason { get; set; } = "<size=60>SteamAPI check</size>\n<size=30>[Anti sus acc]</size>\nYour acc have too many bans";

    }
}
