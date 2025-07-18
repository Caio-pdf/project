using Microsoft.Extensions.Options;

namespace ApiAutomation.Configuration
{
    public class CredentialsSettings
    {
        public string? MerchantToken { get; set; }

        public void EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(MerchantToken))
            {
                throw new OptionsValidationException(
                    "Credentials:MerchantToken",
                    typeof(CredentialsSettings),
                    new[] { "A configuração 'Credentials:MerchantToken' não pode ser nula ou vazia." }
                );
            }
        }
    }
}