using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTracker
{
    public class EntitlementTokens
    {
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string EntitlementToken { get; set; } = string.Empty;

        [JsonProperty("subject")]
        public string UserPlayerId { get; set; } = string.Empty;
    }
}
