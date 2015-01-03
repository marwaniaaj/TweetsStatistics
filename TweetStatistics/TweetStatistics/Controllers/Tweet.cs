using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetStatistics.Controllers
{
    public class Tweet
    {
        #region Variable Declarations
        private string _tweetId = String.Empty;
        private string _tweetText = String.Empty;
        private string _tweetScreenName = String.Empty;
        private string _tweetUserName = String.Empty;
        private int _tweetUserFollowersCount = 0;
        private int _tweetUserMentionCount = 0;
        private List<UserMentioned> _tweetUserMentioned = null;
        private static string _mentionUserName = String.Empty;
        private static string _mentionScreenName = String.Empty;
        private int _tweetHashtagsCount = 0;
        private List<string> _hashtags = null;
        private int _tweetRetweetCount = 0;
        private int _tweetFavoriteCount = 0;

        #endregion

        #region Tweet properties
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TweetId
        {
            get { return _tweetId; }
            set { _tweetId = value; }
        }

        public string TweetText 
        {
            get { return _tweetText; }
            set { _tweetText = value;} 
        }
        public string TweetScreenName 
        { 
            get { return _tweetScreenName; }
            set { _tweetScreenName = value; } 
        }
        public string TweetUserName 
        {
            get { return _tweetUserName; }
            set { _tweetUserName = value; }
        }
        public int TweetUserFollowersCount
        {
            get { return _tweetUserFollowersCount; }
            set { _tweetUserFollowersCount = value; }
        }
        public int UserMentionCount
        {
            get { return _tweetUserMentionCount; }
            set { _tweetUserMentionCount = value; }
        }
        public List<UserMentioned> UsersMentioned
        {
            get { return _tweetUserMentioned; }
            set { _tweetUserMentioned = value; }
        }
        public int HashtagsCount
        {
            get { return _tweetHashtagsCount; }
            set { _tweetHashtagsCount = value; }
        }
        public List<string> Hashtags
        {
            get { return _hashtags; }
            set { _hashtags = value; }
        }
        public int RetweetCount
        {
            get { return _tweetRetweetCount; }
            set { _tweetRetweetCount = value; }
        }
        public int FavoriteCount
        {
            get { return _tweetFavoriteCount; }
            set { _tweetFavoriteCount = value; }
        }

        public class UserMentioned
        {
            public string UserName
            {
                get { return _mentionUserName; }
                set { _mentionUserName = value; }
            }
            public string UserScreenName
            {
                get { return _mentionScreenName; }
                set { _mentionScreenName = value; }
            }
        }

        #endregion

        #region Constructors

        /// <summary> Default constructor, with no overloads 
        /// </summary>
        public Tweet()
        {

        }

        /// <summary> constructor with overloads </summary>
        /// <param name="postTweet">Tweet object with data to verify</param>
        public Tweet(Dictionary<string, object> postTweet)
        {
            /// Tweet Id, and Text
            TweetId = postTweet["id_str"].ToString();
            TweetText = postTweet["text"].ToString();

            /// Tweet User
            Dictionary<string, object> user = (Dictionary<string, object>)postTweet["user"];
            TweetScreenName = "@" + user["screen_name"].ToString();
            TweetUserName = user["name"].ToString();
            TweetUserFollowersCount = (Int32)user["followers_count"];

            /// Tweet entities (to get users mentioned, & hashtags)
            Dictionary<string, object> entities = (Dictionary<string, object>)postTweet["entities"];

            /// Users Mentioned in tweet [count, screen name, & user name]
            object[] userMentions = (object[])entities["user_mentions"];
            UserMentionCount = userMentions.Length;

            UsersMentioned = new List<Tweet.UserMentioned>();
            foreach (Dictionary<string, object> userMen in userMentions)
            {
                UsersMentioned.Add(new Tweet.UserMentioned
                {
                    UserScreenName = "@" + userMen["screen_name"].ToString(),
                    UserName = userMen["name"].ToString()
                });
            }

            /// Hashtags [count, & text]
            object[] hashtags = (object[])entities["hashtags"];
            HashtagsCount = hashtags.Length;
            Hashtags = new List<string>();

            foreach (Dictionary<string, object> hashtag in hashtags)
            { Hashtags.Add("#" + hashtag["text"].ToString()); }

            ///retweeted_status [retweet count, favorite count]
            if (postTweet.ContainsKey("retweeted_status"))
            {
                Dictionary<string, object> retweetStatues = (Dictionary<string, object>)postTweet["retweeted_status"];
                RetweetCount = (Int32)retweetStatues["retweet_count"];
                FavoriteCount = (Int32)retweetStatues["favorite_count"];
            }
            else
            { RetweetCount = FavoriteCount = 0; }
        }

        #endregion
    }
    
}