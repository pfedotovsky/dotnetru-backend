using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DotNetRu.Clients.Portable.Services;

namespace TweetAzure
{
    public static class TweetFunction
    {
        [FunctionName("Tweets")]
        [Produces("application/json")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var tweets = await TweetService.Get();
            var json = JsonConvert.SerializeObject(tweets);

            return new OkObjectResult(json);
        }
    }
}
