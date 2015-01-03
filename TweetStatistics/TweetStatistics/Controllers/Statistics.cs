using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetStatistics.Controllers
{
    public class Statistics
    {
        #region Properties
        public List<Tweet> TopRetweets { get; set; }
        public List<Tweet> TopFavoriteTweets { get; set; }
        public List<Tweet> TopFollowedUsers { get; set; }
        public List<Mentioned> TopMentionedUsers { get; set; }
        public List<Hashtaged> TopHashtags { get; set; }
        #endregion

        #region Properties classes
        public class Mentioned
        {
            public string ScreenName { get; set; }
            public string UserName { get; set; }
            public int MentionedCount { get; set; }
        }
        public class Hashtaged
        {
            public string Hashtag { get; set; }
            public int HashtagCount { get; set; }
        }
        #endregion

        /// <summary> Constructor with parameters
        /// </summary>
        /// <param name="tweets"></param>
        public Statistics(MongoCollection<Tweet> tweets)
        {
            /// Extract top 5's
            TopRetweets = tweets.FindAllAs<Tweet>().SetSortOrder(SortBy.Descending("RetweetCount"))
                .GroupBy(s => s.TweetText).Select(s => s.FirstOrDefault()).Take(5).ToList();
            TopFavoriteTweets = tweets.FindAllAs<Tweet>().SetSortOrder(SortBy.Descending("FavoriteCount"))
                .GroupBy(s => s.TweetText).Select(s => s.FirstOrDefault()).Take(5).ToList();
            TopFollowedUsers = tweets.FindAllAs<Tweet>().SetSortOrder(SortBy.Descending("TweetUserFollowersCount"))
                .GroupBy(s => s.TweetScreenName).Select(s => s.FirstOrDefault()).Take(5).ToList();
            List<Tweet> mentionedTweets = tweets.FindAs<Tweet>(Query.GT("UserMentionCount", 0)).ToList();
            List<Tweet> hashtagedTweets = tweets.FindAs<Tweet>(Query.GT("HashtagsCount", 0)).ToList();

            #region Top Mentioned Users
            TopMentionedUsers = new List<Mentioned>();
            foreach (Tweet element in mentionedTweets)
            {
                TopMentionedUsers.Add(new Mentioned
                {
                    ScreenName = element.TweetScreenName,
                    UserName = element.TweetUserName
                });
            }

            foreach (var item in TopMentionedUsers.GroupBy(tweet => tweet.ScreenName, tweet => tweet.UserName)
                                               .Select(tweet => new { screenName = tweet.Key, count = tweet.Count() })
                                               .OrderByDescending(order => order.count))
            {
                TopMentionedUsers[TopMentionedUsers.FindIndex(find => find.ScreenName == item.screenName)].MentionedCount = item.count;
            }
            TopMentionedUsers = TopMentionedUsers.OrderByDescending(order => order.MentionedCount).Take(5).ToList();
            #endregion

            #region Top Hashtaged
            TopHashtags = new List<Hashtaged>();
            foreach (Tweet element in hashtagedTweets)
            {
                foreach (string hashtag in element.Hashtags)
                {
                    TopHashtags.Add(new Hashtaged { Hashtag = hashtag });
                }
            }

            foreach (var item in TopHashtags.GroupBy(tweet => tweet.Hashtag)
                                         .Select(tweet => new { hashtag = tweet.Key, count = tweet.Count() })
                                         .OrderByDescending(order => order.count))
            {
                TopHashtags[TopHashtags.FindIndex(find => find.Hashtag == item.hashtag)].HashtagCount = item.count;
            }
            TopHashtags = TopHashtags.OrderByDescending(order => order.HashtagCount).Take(5).ToList();
            #endregion

        }

        string htmltext = string.Empty;
        public string HTMLFormatter()
        {
            #region Top 5 Retweets
            htmltext = "<span style='font:17px tahoma;font-weight:bold'>Top 5 Retweets</span><br>";
            htmltext += "<table border='1' cellpadding='5' style='font:14px tahoma'>";
            htmltext += "<tr><td style='font-weight:bold'>Tweet</td><td style='font-weight:bold'>Retweets Count</td></tr>";
            foreach (Tweet element in TopRetweets)
            {
                htmltext += "<tr><td>" + element.TweetText + "</td><td>"  + element.RetweetCount + "</td></tr>";
            }
            htmltext += "</table><br/><br/>";
            #endregion

            #region Top 5 Favorite Tweets
            htmltext += "<span style='font:17px tahoma;font-weight:bold'>Top 5 Favorite Tweets</span><br>";
            htmltext += "<table border='1' cellpadding='5' style='font:14px tahoma'>";
            htmltext += "<tr><td style='font-weight:bold'>Tweet</td><td style='font-weight:bold'>Favorites Count</td></tr>";
            foreach (Tweet element in TopFavoriteTweets)
            {
                htmltext += "<tr><td>" + element.TweetText + "</td><td>" + element.FavoriteCount + "</td></tr>";
            }
            htmltext += "</table><br/><br/>";
            #endregion

            #region Top 5 Followed Users
            htmltext += "<span style='font:17px tahoma;font-weight:bold'>Top 5 Followed Users</span><br>";
            htmltext += "<table border='1' cellpadding='5' style='font:14px tahoma'>";
            htmltext += "<tr><td style='font-weight:bold'>User Id</td><td style='font-weight:bold'>User Name</td><td style='font-weight:bold'>Followers Count</td></tr>";
            foreach (Tweet element in TopFollowedUsers)
            {
                htmltext += "<tr><td>" + element.TweetScreenName + "</td><td>"
                         + element.TweetUserName + "</td><td>" + element.TweetUserFollowersCount + "</td></tr>";
            }
            htmltext += "</table><br/><br/>";
            #endregion

            #region Top 5 Mentioned Users
            htmltext += "<span style='font:17px tahoma;font-weight:bold'>Top 5 Mentioned Users</span><br>";
            htmltext += "<table border='1' cellpadding='5' style='font:14px tahoma'>";
            htmltext += "<tr><td style='font-weight:bold'>User Id</td><td style='font-weight:bold'>User Name</td><td style='font-weight:bold'>Mentions Count</td></tr>";
            foreach (Mentioned element in TopMentionedUsers)
            {
                htmltext += "<tr><td>" + element.ScreenName + "</td><td>"
                         + element.UserName + "</td><td>" + element.MentionedCount + "</td></tr>";
            }
            htmltext += "</table><br/><br/>";
            #endregion

            #region Top 5 Hashtags
            htmltext += "<span style='font:17px tahoma;font-weight:bold'>Top 5 Hashtags</span><br>";
            htmltext += "<table border='1' cellpadding='5' style='font:14px tahoma'>";
            htmltext += "<tr><td style='font-weight:bold'>Hashtag</td><td style='font-weight:bold'>Hashtag Count</td></tr>";
            foreach (Hashtaged element in TopHashtags)
            {
                htmltext += "<tr><td>" + element.Hashtag + "</td><td>"
                         + element.HashtagCount + "</td></tr>";
            }
            htmltext += "</table><br/><br/>";
            #endregion

            return htmltext;
        }
    }
}