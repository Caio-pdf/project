using Microsoft.Extensions.Options;

namespace ApiAutomation.Configuration
{
    public class ApiEndpointsSettings
    {
        public PortalEndpointSettings? Portal { get; set; }
        public PaymentEndpointSettings? Payment { get; set; }

        public void EnsureValid()
        {
            if (Portal == null) throw new OptionsValidationException("ApiEndpoints:Portal", typeof(ApiEndpointsSettings), new[] { "Seção 'ApiEndpoints:Portal' é obrigatória." });
            Portal.EnsureValid();

            if (Payment == null) throw new OptionsValidationException("ApiEndpoints:Payment", typeof(ApiEndpointsSettings), new[] { "Seção 'ApiEndpoints:Payment' é obrigatória." });
            Payment.EnsureValid();
        }
    }

    public class PortalEndpointSettings
    {
        public string? BaseUrl { get; set; }
        public string? GenerateToken { get; set; }

        public void EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl)) throw new OptionsValidationException("ApiEndpoints:Portal:BaseUrl", typeof(PortalEndpointSettings), new[] { "'ApiEndpoints:Portal:BaseUrl' não pode ser nulo ou vazio." });
            if (string.IsNullOrWhiteSpace(GenerateToken)) throw new OptionsValidationException("ApiEndpoints:Portal:GenerateToken", typeof(PortalEndpointSettings), new[] { "'ApiEndpoints:Portal:GenerateToken' não pode ser nulo ou vazio." });
        }
    }

    public class PaymentEndpointSettings
    {
        public string? Authorization { get; set; }
        public string? AuthorizationEcommerce { get; set; }
        public string? PreAuthorization { get; set; }
        public string? PreAuthorizationEcommerce { get; set; }

        public void EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(Authorization))
                throw new OptionsValidationException("ApiEndpoints:Payment:Authorization", typeof(PaymentEndpointSettings), new[] { "'ApiEndpoints:Payment:Authorization' não pode ser nulo ou vazio." });
            if (string.IsNullOrWhiteSpace(AuthorizationEcommerce))
                throw new OptionsValidationException("ApiEndpoints:Payment:AuthorizationEcommerce", typeof(PaymentEndpointSettings), new[] { "'ApiEndpoints:Payment:AuthorizationEcommerce' não pode ser nulo ou vazio." });
            if (string.IsNullOrWhiteSpace(PreAuthorization))
                throw new OptionsValidationException("ApiEndpoints:Payment:PreAuthorization", typeof(PaymentEndpointSettings), new[] { "'ApiEndpoints:Payment:PreAuthorization' não pode ser nulo ou vazio." });
            if (string.IsNullOrWhiteSpace(PreAuthorizationEcommerce))
                throw new OptionsValidationException("ApiEndpoints:Payment:PreAuthorizationEcommerce", typeof(PaymentEndpointSettings), new[] { "'ApiEndpoints:Payment:PreAuthorizationEcommerce' não pode ser nulo ou vazio." });
        }
    }
}