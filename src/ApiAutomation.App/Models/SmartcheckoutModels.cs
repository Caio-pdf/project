using Newtonsoft.Json;

namespace ApiAutomation.App.Models
{
    public class CreateSmartCheckoutRequest
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("emailNotification")]
        public string? EmailNotification { get; set; }

        [JsonProperty("phoneNotification")]
        public string? PhoneNotification { get; set; }

        [JsonProperty("installmentNumber")]
        public int InstallmentNumber { get; set; }

        [JsonProperty("installmentType")]
        public string? InstallmentType { get; set; }
    }

    public class CreateSmartCheckoutResponse
    {
        // Aqui está o nosso tesouro!
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("SmartCheckoutUrl")]
        public string? smartCheckoutUrl { get; set; }

        public bool Success { get; set; }

        [JsonProperty("traceKey")]
        public required string TraceKey { get; set; }
        public List<ApiError>? Errors { get; set; }
    }
}