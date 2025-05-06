using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PangyaAPI.Discord
{
    public class PlayerDiscord : HttpClient
    {
        string _link = "https://discord.com/api/webhooks/1363188276957155562/eXsGVgP9s5ltyX59jU2fLLlLcLxgjj-Hqpe_xcOpTeGgp-1wxGFfkIhSVVJuBb_v4DYI";
        public PlayerDiscord()
        { }

        public PlayerDiscord(HttpMessageHandler handler) : base(handler)
        {
        }

        public PlayerDiscord(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
        }
        public async Task SendMessage(string msg)
        {
            var payload = new
            {
                content = msg
            };

            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            await PostAsync(_link, stringContent);
        }
    }
    public static class DiscordWebhook
    {
        public static async Task ChatLog(string content)
        {
            await new PlayerDiscord().SendMessage(content);
        }
    }
}
