using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Script.Serialization;
using TweetStatistics.App_Start;
using TweetStatistics.Properties;

namespace TweetStatistics.Controllers
{
    public class TweetsController : ApiController
    {
        TweetsContext _context = new TweetsContext();

        [HttpGet]
        [Route("Tweets")]
        public async Task<HttpResponseMessage> Get()
        {
            /// Get application-only authentication
            AuthenticationResponse authenticationResponse = await Task.Run(() => GetAuthenticationToken());

            /// Building URL
            string srchStr = "pluralsight";
            var twitterClient = new HttpClient();
            var uri = new Uri(string.Format(ConfigurationManager.AppSettings["twitterSearchLink"], srchStr));

            /// Accessing URL using authentication
            twitterClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", authenticationResponse.AccessToken));

            var task = await twitterClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
            task.EnsureSuccessStatusCode();
            string content = await task.Content.ReadAsStringAsync();

            /// Deserialize content
            object jsonDocument = new JavaScriptSerializer().Deserialize<object>(content);
            if (jsonDocument != null)
            {
                _context.Tweets.RemoveAll();
                object[] statuses = (object[])((Dictionary<string, object>)jsonDocument)["statuses"];
                #region foreach tweet in statuses
                foreach (Dictionary<string, object> tweet in statuses)
                {
                    /// insert tweet object into MongoDb
                    _context.Tweets.Insert(new Tweet(tweet));
                }
                #endregion
            }

            /// Extract statistcs
            Statistics statistics = new Statistics(_context.Tweets);

            string result = statistics.HTMLFormatter();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(result, Encoding.UTF8, "text/html");
            return response;
        }

        private async Task<AuthenticationResponse> GetAuthenticationToken()
        {
            var authClient = new HttpClient();

            var combinedKeys = String.Format("{0}:{1}",
                WebUtility.UrlEncode(ConfigurationManager.AppSettings["twitterConsumerKey"]),
                WebUtility.UrlEncode(ConfigurationManager.AppSettings["twitterConsumerSecret"]));

            authClient.DefaultRequestHeaders.Add("Authorization", string.Format("Basic {0}",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(combinedKeys))));

            var data = new List<KeyValuePair<string, string>> { 
            new KeyValuePair<string, string>("grant_type", "client_credentials")};

            var response = await authClient.PostAsync(
                new Uri(ConfigurationManager.AppSettings["authenticationURI"]),
                new FormUrlEncodedContent(data));

            AuthenticationResponse authenticationResponse;
            using (response)
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Did not work!");
                var content = await response.Content.ReadAsStringAsync();
                authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
                if (authenticationResponse.TokenType != "bearer")
                    throw new Exception("wrong result type");
            }
            return authenticationResponse;
        }
    }
}
