using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace PushNotifications
{
    public static class PushFunction
    {
        [FunctionName("Push")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{channelName}")]
            HttpRequest req,
            string channelName,
            ILogger log)
        {
            log.LogInformation("Triggered Push Notification");

            string androidResult = await SendPushForAndroid(channelName);

            return new OkObjectResult($"Push Notification for Android sent: " + androidResult);
        }

        private static async Task<string> SendPushForAndroid(string channelName)
        {
            var contentString =
$@"{{
  'to': '/topics/{channelName}',
    'data': {{
       'my_custom_key': 'my_custom_value'
     }}
}}";
            var content = new StringContent(contentString.Replace("'", "\""), Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();

            var authorizationKey = System.Environment.GetEnvironmentVariable("AuthorizationKey");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);

            var result = await httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", content);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
