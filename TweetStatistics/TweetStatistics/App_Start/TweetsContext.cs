using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetStatistics.Controllers;
using TweetStatistics.Properties;

namespace TweetStatistics.App_Start
{
    public class TweetsContext
    {
        public MongoDatabase database;
        
        public TweetsContext()
        {
            var client = new MongoClient(Settings.Default.TweetsStatisticsConnectionString);
            var server = client.GetServer();
            database = server.GetDatabase(Settings.Default.TweetsStatisticsDbName);
        }
        public MongoCollection<Tweet> Tweets
        {
            get { return database.GetCollection<Tweet>("Tweets"); }
        }

    }
}