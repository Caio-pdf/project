using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiAutomation.App.Configuration
{

    public class TransactionEndpointsSettings
    {
        [JsonProperty("transactionEndpoints")]
        public  Dicttionary<string, SourceEndpoints>? Transaction { get; set; }
    }

    public class SourceEndpoints
    {
        [JsonProperty("Authorization")]
        public string? Authorization { get; set; }

        [JsonProperty("PreAuthorization")]
        public string? PreAuthorization { get; set; }
    }
}