using System.Net.Http.Json;
using System.Text.Json;
using ApiAutomation.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Globalization;

namespace ApiAutomation.App.Services.Implementations
{
    public class AuthTokenDetails
    {
        public required string AccessToken { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
    }

    public class TokenResponse
    {
        public bool Success { get; set; }
        public string? GeneratedToken { get; set; }
    }

    public partial class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly CredentialsSettings _credentials;
        private readonly TimeSpan _assumedTokenDuration;
        private readonly Uri _authServiceUrl;
        private readonly JwtSecurityTokenHandler _jwtHandler;
        private readonly TimeZoneInfo _brasiliaTimeZone;

        public AuthenticationService(
            HttpClient httpClient,
            IConfiguration configuration,
            IOptions<CredentialsSettings> credentialsOptions,
            IOptions<AuthenticationSettings> authSettingsOptions,
            ILogger<AuthenticationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _credentials = credentialsOptions?.Value ?? throw new ArgumentNullException(nameof(credentialsOptions), "Configuração 'Credentials' não encontrada ou inválida.");
            var authSettings = authSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(authSettingsOptions), "Configuração 'Authentication' não encontrada ou inválida.");

            _jwtHandler = new JwtSecurityTokenHandler();

            try
            {

                _brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("Fuso horário 'America/Sao_Paulo' não encontrado. Tentando 'E. South America Standard Time'.");
                try
                {
                    _brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                catch (TimeZoneNotFoundException ex)
                {
                    _logger.LogError(ex, "Fuso horário de Brasília não encontrado. Os logs de data/hora de expiração podem não usar o fuso de Brasília.");
                    _brasiliaTimeZone = TimeZoneInfo.Local;
                }
            }


            string? portalBaseUrl = configuration["ApiEndpoints:Portal:BaseUrl"];
            string? portalGenerateTokenPath = configuration["ApiEndpoints:Portal:GenerateToken"];

            _logger.LogDebug("Lendo configuração do Portal diretamente: BaseUrl='{PortalBaseUrl}', GenerateTokenPath='{PortalTokenPath}'", portalBaseUrl, portalGenerateTokenPath);

            if (string.IsNullOrWhiteSpace(portalBaseUrl))
                throw new InvalidOperationException("Configuração 'ApiEndpoints:Portal:BaseUrl' não encontrada ou vazia em IConfiguration.");
            if (string.IsNullOrWhiteSpace(portalGenerateTokenPath))
                throw new InvalidOperationException("Configuração 'ApiEndpoints:Portal:GenerateToken' não encontrada ou vazia em IConfiguration.");
            if (string.IsNullOrWhiteSpace(_credentials.MerchantToken))
                throw new InvalidOperationException("Credentials:MerchantToken não configurada.");
            if (authSettings.TokenAssumedDurationMinutes <= 0)
                throw new InvalidOperationException("Authentication:TokenAssumedDurationMinutes deve ser um valor positivo.");

            _assumedTokenDuration = TimeSpan.FromMinutes(authSettings.TokenAssumedDurationMinutes);

            try
            {
                Uri baseUri = new Uri(portalBaseUrl, UriKind.Absolute);
                _authServiceUrl = new Uri(baseUri, portalGenerateTokenPath);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Erro ao construir a URI do serviço de autenticação a partir de BaseUrl '{BaseUrl}' e Endpoint '{Endpoint}' LIDOS DIRETAMENTE",
                                 portalBaseUrl, portalGenerateTokenPath);
                throw new InvalidOperationException($"Formato inválido para a combinação de ApiEndpoints:Portal:BaseUrl ('{portalBaseUrl}') e ApiEndpoints:Portal:GenerateToken ('{portalGenerateTokenPath}').", ex);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Erro ao construir URI: BaseUrl ou Path estava nulo após validação. BaseUrl='{BaseUrl}', Path='{Path}'", portalBaseUrl, portalGenerateTokenPath);
                throw;
            }

            _logger.LogInformation("AuthenticationService inicializado. AuthUrl: {AuthUrl}, Duração Assumida (Fallback) Token: {Duration}",
                                   _authServiceUrl, _assumedTokenDuration);
        }

        public async Task<AuthTokenDetails?> FetchNewTokenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { merchantToken = _credentials.MerchantToken };
                _logger.LogInformation("Buscando novo token de autenticação em {AuthUrl}...", _authServiceUrl);
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_authServiceUrl, payload, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

                    if (tokenResponse?.Success == true && !string.IsNullOrEmpty(tokenResponse.GeneratedToken))
                    {
                        _logger.LogInformation("Novo token de autenticação obtido com sucesso da API.");
                        DateTimeOffset expiresAtUtc;

                        if (_jwtHandler.CanReadToken(tokenResponse.GeneratedToken))
                        {
                            try
                            {
                                var jwtToken = _jwtHandler.ReadJwtToken(tokenResponse.GeneratedToken);
                                var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp || claim.Type == "exp");

                                if (expClaim != null && long.TryParse(expClaim.Value, out long expTimestamp))
                                {
                                    expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expTimestamp);

                                    DateTimeOffset expiresAtBrasilia = TimeZoneInfo.ConvertTime(expiresAtUtc, _brasiliaTimeZone);

                                    string expiresAtBrasiliaFormatted = expiresAtBrasilia.ToString("dd/MM/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);

                                    _logger.LogInformation(
                                        "Data de expiração lida do token JWT: {ExpiresAtBrasilia} (Horário de Brasília) / {ExpiresAtUtc} (UTC). Duração real: {ActualDuration}",
                                        expiresAtBrasiliaFormatted,
                                        expiresAtUtc,
                                        expiresAtUtc - DateTimeOffset.UtcNow);
                                }
                                else
                                {
                                    _logger.LogWarning("Claim 'exp' não encontrada ou inválida no token JWT. Usando duração assumida de {AssumedDuration}.", _assumedTokenDuration);
                                    expiresAtUtc = DateTimeOffset.UtcNow.Add(_assumedTokenDuration);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Erro ao tentar ler o token JWT para extrair 'exp'. Usando duração assumida.");
                                expiresAtUtc = DateTimeOffset.UtcNow.Add(_assumedTokenDuration);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Não foi possível ler o token JWT retornado (formato inválido?). Usando duração assumida de {AssumedDuration}.", _assumedTokenDuration);
                            expiresAtUtc = DateTimeOffset.UtcNow.Add(_assumedTokenDuration);
                        }
                        return new AuthTokenDetails
                        {
                            AccessToken = tokenResponse.GeneratedToken,
                            ExpiresAt = expiresAtUtc
                        };
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogError("Falha ao obter token: Resposta da API indicou falha ou token ausente. Success={Success}, GeneratedToken Empty={IsTokenEmpty}, Body={ErrorBody}",
                            tokenResponse?.Success, string.IsNullOrEmpty(tokenResponse?.GeneratedToken), errorBody);
                        return null;
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Erro na requisição de token: Status={StatusCode}, Reason={ReasonPhrase}, Body={ErrorBody}",
                        response.StatusCode, response.ReasonPhrase, errorBody);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede ao buscar token em {AuthUrl}.", _authServiceUrl);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao desserializar a resposta do token de {AuthUrl} (System.Text.Json).", _authServiceUrl);
                return null;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout ao buscar token em {AuthUrl}.", _authServiceUrl);
                return null;
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Busca de token cancelada pelo solicitante.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar token de autenticação.");
                return null;
            }
        }
        public string GetServiceUrlForLogging() => _authServiceUrl?.ToString() ?? "URL não inicializada";
    }
}