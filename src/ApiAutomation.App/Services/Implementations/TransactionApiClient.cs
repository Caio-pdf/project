using ApiAutomation.App.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiAutomation.App.Services.Implementations
{
    public class TransactionApiClient : ITransactionApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionApiClient> _logger;

        public TransactionApiClient(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<TransactionApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpointKey, TRequest payload, CancellationToken cancellationToken)
        {
            var configPath = $"ApiEndpoints:Payment:{endpointKey}";
            var endpointPath = _configuration[configPath];

            if (string.IsNullOrWhiteSpace(endpointPath))
            {
                var errorMessage = $"O caminho do endpoint para a chave '{endpointKey}' não foi encontrado na configuração (caminho buscado: '{configPath}'). Verifique o appsettings.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = _configuration["ApiEndpoints:Payment:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    var errorMessage = "BaseUrl não configurada para ApiEndpoints:Payment:BaseUrl";
                    _logger.LogError(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
                _httpClient.BaseAddress = new Uri(baseUrl);
            }

            var requestUri = new Uri(_httpClient.BaseAddress, endpointPath.TrimStart('/'));

            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando POST para: {RequestUri}", requestUri);
            _logger.LogTrace("Payload: {Payload}", jsonPayload);

            try
            {
                var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Resposta bem-sucedida de {RequestUri} ({StatusCode})", requestUri, response.StatusCode);
                _logger.LogTrace("Response Body: {Body}", responseBody);

                return JsonConvert.DeserializeObject<TResponse>(responseBody)!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha de comunicação (HTTP) ao fazer POST para {RequestUri}.", requestUri);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha ao deserializar a resposta JSON de {RequestUri}", requestUri);
                throw;
            }
        }
    }
}