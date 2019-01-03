using NLog;
using System.Text.RegularExpressions;

namespace DotNetRu.Clients.Portable.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Model;

    using LinqToTwitter;
    using TweetAzure;

    public class TweetService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns tweets by SpdDotNet/DotNetRu (if it's retweet then original tweet is returned instead of retweet)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Tweet>> Get()
        {
            try
            {
                var auth = new ApplicationOnlyAuthorizer
                {
                    CredentialStore =
                                       new InMemoryCredentialStore
                                       {
                                           ConsumerKey =
                                                   "ho0v2B1bimeufLqI1rA8KuLBp",
                                           ConsumerSecret =
                                                   "RAzIHxhkzINUxilhdr98TWTtjgFKXYzkEhaGx8WJiBPh96TXNK"
                                       },
                };
                await auth.AuthorizeAsync();

                var twitterContext = new TwitterContext(auth);

                var spbDotNetTweets =
                    await (from tweet in twitterContext.Status
                           where tweet.Type == StatusType.User && tweet.ScreenName == "spbdotnet"
                                                               && tweet.TweetMode == TweetMode.Extended
                           select tweet).ToListAsync();

                var dotnetruTweets =
                    await (from tweet in twitterContext.Status
                           where tweet.Type == StatusType.User && tweet.ScreenName == "DotNetRu"
                                                               && tweet.TweetMode == TweetMode.Extended
                           select tweet).ToListAsync();

                var unitedTweets = spbDotNetTweets.Union(dotnetruTweets).Where(tweet => !tweet.PossiblySensitive).Select(GetTweet);

                var tweetsWithoutDuplicates = unitedTweets.GroupBy(tw => tw.StatusID).Select(g => g.First());

                var sortedTweets = tweetsWithoutDuplicates.OrderByDescending(x => x.CreatedDate).ToList();

                return sortedTweets;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new List<Tweet>();
        }

        private static Tweet GetTweet(Status tweet)
        {
            var sourceTweet = tweet.RetweetedStatus.StatusID == 0 ? tweet : tweet.RetweetedStatus;

            var urlLinks =
                sourceTweet.Entities.UrlEntities.Select(t => new KeyValuePair<string, string>(t.Url, t.DisplayUrl)).ToList();

            var profileImage = sourceTweet.User?.ProfileImageUrl.Replace("http://", "https://");
            if (profileImage != null)
            {
                //normal image is 48x48, bigger image is 73x73, see https://developer.twitter.com/en/docs/accounts-and-users/user-profile-images-and-banners
                profileImage = Regex.Replace(profileImage, @"(.+)_normal(\..+)", "$1_bigger$2");
            }

            return new Tweet(sourceTweet.StatusID)
            {                
                TweetedImage =
                               tweet.Entities?.MediaEntities.Count > 0
                                   ? tweet.Entities?.MediaEntities?[0].MediaUrlHttps ?? string.Empty
                                   : string.Empty,
                NumberOfLikes = sourceTweet.FavoriteCount,
                NumberOfRetweets = sourceTweet.RetweetCount,
                ScreenName = sourceTweet.User?.ScreenNameResponse ?? string.Empty,
                Text = sourceTweet.FullText.ConvertToUsualUrl(urlLinks),
                Name = sourceTweet.User?.Name,
                CreatedDate = tweet.CreatedAt,
                Url = $"https://twitter.com/{sourceTweet.User?.ScreenNameResponse}/status/{tweet.StatusID}",
                Image = profileImage
            };
        }
    }
}