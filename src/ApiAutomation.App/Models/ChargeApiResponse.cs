using System.Collections.Generic;

namespace ApiAutomation.App.Models
{
    public class ChargeApiResponse
    {
        public bool Success { get; set; }
        public ChargeResult? Charge { get; set; }
        public List<ApiError>? Errors { get; set; }
    }

    public class ApiError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}