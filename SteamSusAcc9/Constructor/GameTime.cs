using Exiled.API.Features;

namespace SteamSusAcc.Constructor
{
    public class GameTime
    {
        public int MinHours { get; set; }
        public string KickReason { get; set; }
        public string CheckFailText { get; set; }
        public string WebhookText { get; set; }
        public GameTime() { }
        public GameTime(int minHours, string kickReason, string checkFailText, string webhookText)
        {
            MinHours = minHours;
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
            //Plugin.webhook.Send(WebhookText.Replace("%playerinfo%", Plugin.plugin.GetPlayerInfo(player)));
        }
    }
}
