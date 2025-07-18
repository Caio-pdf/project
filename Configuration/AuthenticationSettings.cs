using Microsoft.Extensions.Options;

namespace ApiAutomation.Configuration
{
    public class AuthenticationSettings
    {
        public int TokenAssumedDurationMinutes { get; set; }

        public void EnsureValid()
        {
            if (TokenAssumedDurationMinutes <= 0)
            {
                throw new OptionsValidationException(
                    "Authentication:TokenAssumedDurationMinutes",
                    typeof(AuthenticationSettings),
                    new[] { "A configuração 'Authentication:TokenAssumedDurationMinutes' deve ser um valor positivo." }
                );
            }
        }
    }
}