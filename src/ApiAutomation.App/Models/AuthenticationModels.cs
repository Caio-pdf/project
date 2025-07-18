using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ApiAutomation.App.Models 
{
    public class AuthorizationRequest
    {
        [JsonObject("charge")]
        public class BaseChargePayload
        {
            [JsonPropertyName("merchantChargeId")]
            public string? MerchantChargeId { get; set; }

            [JsonPropertyName("customer")]
            public CustomerDetails? Customer { get; set; }

            [JsonPropertyName("transactions")]
            public List<TransactionDetails>? Transactions { get; set; }
        }
    }
    public class AuthorizationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("charge")]
        public ChargeResponsePayload? Charge { get; set; }

        [JsonPropertyName("errors")]
        public List<ApiError>? Errors { get; set; }
    }
    public class ChargeResponsePayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("merchantChargeId")]
        public string? MerchantChargeId { get; set; }
    }
    public class AuthorizationResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? Id { get; set; }

        public string? MerchantChargeId { get; set; }
    }
}
