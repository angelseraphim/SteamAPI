using Exiled.API.Features;

namespace SteamSusAcc.Constructor
{
    public class Privacy
    {
        public string KickReason { get; set; }
        public string CheckFailText { get; set; }
        public string WebhookText { get; set; }
        public Privacy() { }
        public Privacy(string kickReason, string checkFailText, string webhookText)
        {
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
