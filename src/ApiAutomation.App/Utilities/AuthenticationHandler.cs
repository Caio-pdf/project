using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace ApiAutomation.App.Utilities
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<AuthenticationHandler> _logger;

        public AuthenticationHandler(ITokenProvider tokenProvider, ILogger<AuthenticationHandler> logger)
            : base()
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("AuthenticationHandler: Obtendo token para a requisição {RequestMethod} {RequestUri}", request.Method, request.RequestUri);

                string rawToken = await _tokenProvider.GetValidTokenAsync(cancellationToken);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawToken);

                _logger.LogDebug("AuthenticationHandler: Cabeçalho Authorization adicionado.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthenticationHandler: Falha ao obter/adicionar token de autenticação. A requisição para {RequestUri} seguirá sem token.", request.RequestUri);
            }

            _logger.LogDebug("AuthenticationHandler: Enviando requisição para o próximo handler...");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}