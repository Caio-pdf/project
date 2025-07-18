using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiAutomation.App.Models
{
    public abstract class BaseChargePayload
    {
        [JsonProperty("merchantChargeId")]
        public string? MerchantChargeId { get; set; }
        
        [JsonProperty("customer")]
        public CustomerDetails? Customer { get; set; }

        [JsonProperty("transactions")]
        public List<TransactionDetails>? Transactions { get; set; }

        [JsonProperty("source")]
        public abstract int Source { get; }
    }

    public class EcommerceChargePayload : BaseChargePayload
    {
        [JsonProperty("lateCapture")]
        public bool LateCapture { get; set; }

        public override int Source => 8; 
    }

    public class SmartCheckoutChargePayload : BaseChargePayload
    {
        [JsonProperty("smartCheckoutId")]
        public string? SmartCheckoutId { get; set; }
        
        public override int Source => 10;
    }

    public class CustomerDetails
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("document")]
        public string? Document { get; set; }

        [JsonObject("Phone")]
        public class Phone
        {
            [JsonProperty("countryCode")]
            public string? CountryCode { get; set; }

            [JsonProperty("areaCode")]
            public string? AreaCode { get; set; }

            [JsonProperty("number")]
            public string? Number { get; set; }

            [JsonProperty("type")]
            public string? Type { get; set; }
        }
    }

    public class TransactionDetails
    {
        
    }
    public class CardDetails { /* ... */ }

    public class ChargeRequest<T> where T : BaseChargePayload
    {
        [JsonProperty("charge")]
        public T Charge { get; set; }

        public ChargeRequest(T charge)
        {
            Charge = charge;
        }
    }
}