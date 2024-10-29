using Exiled.API.Features;

namespace SteamSusAcc.Constructor
{
    public class Age
    {
        public int MinDays { get; set; }
        public string KickReason { get; set; }
        public string CheckFailText { get; set; }
        public string WebhookText { get; set; }
        public Age() { }
        public Age(int minDays, string kickReason, string checkFailText, string webhookText)
        {
            MinDays = minDays;
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
