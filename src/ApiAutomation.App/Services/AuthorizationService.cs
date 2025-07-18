using ApiAutomation.App.Models;
using ApiAutomation.App.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ApiAutomation.App.Services
{
    public class AuthorizationService
    {
        private readonly ITransactionApiClient _apiClient;
        private readonly ITransactionRequestFactory _requestFactory;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ITransactionApiClient apiClient,
            ITransactionRequestFactory requestFactory,
            ILogger<AuthorizationService> logger)
        {
            _apiClient = apiClient;
            _requestFactory = requestFactory;
            _logger = logger;
        }

        public async Task<ChargeResult> AuthorizeAsync(TransactionType type, string endpointKey, Dictionary<string, object>? parameters = null)
        {
            _logger.LogInformation("Iniciando autorização para o tipo '{Type}' no endpoint '{EndpointKey}'.", type, endpointKey);

            try
            {
                var requestPayload = _requestFactory.CreateRequest(type, parameters);

                var response = await _apiClient.PostAsync<object, ChargeApiResponse>(endpointKey, requestPayload, default);

                if (response?.Success == true && response.Charge != null)
                {
                    _logger.LogInformation("Autorização ({Type}) bem-sucedida para o endpoint '{EndpointKey}'. ChargeId: {ChargeId}", type, endpointKey, response.Charge.Id);
                    return response.Charge;
                }
                else
                {
                    string errorMessage = response?.Errors?.FirstOrDefault()?.Message ?? "A API retornou uma falha sem detalhes.";
                    _logger.LogError("Falha na autorização ({Type}) para o endpoint '{EndpointKey}': {ErrorMessage}", type, endpointKey, errorMessage);
                    return new ChargeResult { Success = false, Message = $"Falha API ({type}): {errorMessage}", Id = response?.Charge?.Id, MerchantChargeId = response?.Charge?.MerchantChargeId };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado em AuthorizeAsync para o tipo '{Type}' e endpoint '{EndpointKey}'.", type, endpointKey);
                return new ChargeResult { Success = false, Message = $"Erro inesperado ({ex.GetType().Name}) ao processar {type}." };
            }
        }
    }
}