using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetStatistics.Controllers
{
    class AuthenticationResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
