using Exiled.API.Features;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace SteamSusAcc
{
    public class Webhook
    {
        public void Send(string Text, string Title = null, string WebhookText = null, string AvatarURL = null, string ImageURL = null, int Color = 0xff0000)
        {
            if (Plugin.plugin.Config.DiscordWebHook.IsEmpty())
                return;

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

                client.PostAsync(Plugin.plugin.Config.DiscordWebHook, new StringContent(json, Encoding.UTF8, "application/json"));
            }
            catch
            {
                Log.Error("Webhook is wrong. Please check your configuration");
            }
        }
    }
}
