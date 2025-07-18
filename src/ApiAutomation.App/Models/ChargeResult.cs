using System.Text.Json.Serialization;

namespace ApiAutomation.App.Models
{
    public class ChargeResult
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("merchantChargeId")]
        public string? MerchantChargeId { get; set; }
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }

    }
}