using System.Collections.Generic;
using System.Linq;
using DotNetRu.Clients.Portable.Model;
using DotNetRu.Clients.Portable.Services;
using Xunit;

namespace TweetAzure.Tests
{
    /// <summary>
    /// Tests for <see cref="TweetService"/>
    /// </summary>
    public class TweetServiceTests
    {
        [Fact]
        public async void TestGetOriginalTweets()
        {
            // Act
            var tweets = await TweetService.Get().ConfigureAwait(false);

            // Assert
            Assert.NotNull(tweets);
            Assert.NotEmpty(tweets);
            var originalTweets = new List<Tweet>();
            foreach (var tweet in tweets)
            {
                var name = tweet.Name;
                var text = tweet.Text;
                var retweetsCount = tweet.NumberOfRetweets;
                var likesCount = tweet.NumberOfLikes;

                var sameTweet = originalTweets.FirstOrDefault(tw =>
                    tw.Name == name && tw.Text == text && tw.NumberOfRetweets == retweetsCount &&
                    tw.NumberOfLikes == likesCount);
                Assert.Null(sameTweet);

                originalTweets.Add(tweet);
            }
        }        
    }
}
