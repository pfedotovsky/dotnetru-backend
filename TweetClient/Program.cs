using Flurl;
using Flurl.Http;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TweetClient
{
    public class Program
    {
        public static string BaseURL = "https://tweetazure.azurewebsites.net/api";

        public static string ResourceName = "RestSharp";

        public static async Task Main()
        {
            var client = new RestClient(BaseURL);
            var request = new RestRequest(ResourceName, Method.GET, DataFormat.Json);
            var tweetResponse = client.Execute(request);

            var restSharpContent = tweetResponse.Content;

            // TODO fails
            SimpleJson.SimpleJson.DeserializeObject<Tweet>(restSharpContent);

            // Http Client + RestSharp deserialization
            var httpClient = new HttpClient();
            var httpContent = await httpClient.GetStringAsync(BaseURL + "/" + ResourceName);

            var tweet = SimpleJson.SimpleJson.DeserializeObject<Tweet>(httpContent);

            // Flurl
            var tweet2 = await BaseURL
                .AppendPathSegment(ResourceName)
                .GetJsonAsync<Tweet>();

            Console.ReadLine();
        }
    }
}
