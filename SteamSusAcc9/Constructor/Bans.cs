using Exiled.API.Features;

namespace SteamSusAcc.Constructor
{
    public class Bans
    {
        public bool Disconnect { get; set; }
        public int MinVacBans { get; set; }
        public int MinGameBans { get; set; }
        public int MinTotalBans { get; set; }
        public string KickReason { get; set; }
        public string CheckFailText { get; set; }
        public string WebhookText { get; set; }
        public Bans() { }
        public Bans(bool disconnect, int minVacBans, int minGameBans, int minTotalBans, string kickReason, string checkFailText, string webhookText)
        {
            Disconnect = disconnect;
            MinVacBans = minVacBans;
            MinGameBans = minGameBans;
            MinTotalBans = minTotalBans;
            KickReason = kickReason;
            CheckFailText = checkFailText;
            WebhookText = webhookText;
        }
        public void Apply(Player player, bool IsSeccess)
        {
            if (IsSeccess)
                player.Disconnect(KickReason);
            else
                player.Disconnect(CheckFailText);
            Plugin.webhook.Send(WebhookText.Replace("%playerinfo%", Plugin.plugin.GetPlayerInfo(player)));
        }
    }
}
