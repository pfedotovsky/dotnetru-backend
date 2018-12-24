using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Octokit;
using System.Linq;
using DotNetRu.DataStore.Audit.XmlEntities;

namespace PushNotifications
{
    public static class UpdateFunction
    {
        private const int DotNetRuAppRepositoryID = 89862917;

        [FunctionName("Update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string fromCommitSha = req.Query["fromCommitSha"];

            log.LogInformation($"C# HTTP trigger function processed a request: {fromCommitSha}");

            var client = new GitHubClient(new ProductHeaderValue("DotNetRu"));

            var reference = await client.Git.Reference.Get(DotNetRuAppRepositoryID, "heads/master");
            var latestMasterCommitSha = reference.Object.Sha;

            var contentUpdate = await client.Repository.Commit.Compare(
                                    DotNetRuAppRepositoryID,
                                    fromCommitSha,
                                    latestMasterCommitSha);

            var httpClient = new HttpClient();

            var streamTasks = contentUpdate.Files.Select(
                async file => new
                {
                    file.Filename,
                    Content = await httpClient.GetStringAsync(file.RawUrl).ConfigureAwait(false)
                });
            var fileContents = await Task.WhenAll(streamTasks);

            var xmlFiles = fileContents.Where(x => x.Filename.EndsWith(".xml")).ToList();

            var meetups = xmlFiles.Where(x => x.Filename.Contains("meetups")).Select(x => x.Content.Deserialize<MeetupEntity>());
            var talks = xmlFiles.Where(x => x.Filename.Contains("talks")).Select(x => x.Content.Deserialize<TalkEntity>());
            var speakers = xmlFiles.Where(x => x.Filename.Contains("speakers")).Select(x => x.Content.Deserialize<SpeakerEntity>());
            var friends = xmlFiles.Where(x => x.Filename.Contains("friends")).Select(x => x.Content.Deserialize<FriendEntity>());
            var venues = xmlFiles.Where(x => x.Filename.Contains("venues")).Select(x => x.Content.Deserialize<VenueEntity>());

            var updateContent = new UpdateContent
            {
                Meetups = meetups.ToArray(),
                Talks = talks.ToArray(),
                Speakers = speakers.ToArray(),
                Friends = friends.ToArray(),
                Venues = venues.ToArray()
            };

            // TODO speaker photos - just an array to cache?
            return new JsonResult(updateContent);
        }
    }
}