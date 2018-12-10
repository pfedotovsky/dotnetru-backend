using DotNetRu.Clients.Portable.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace TweetAzure
{
    public static class RestSharpFunction
    {
        [FunctionName("RestSharp")]
        [Produces("application/json")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var tweets = await TweetService.Get();

            var json = JsonConvert.SerializeObject(new { tweets.First().TweetedImage });

            return new OkObjectResult(json);
        }
    }
}
