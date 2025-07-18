using ApiAutomation.App.Models;
using ApiAutomation.App.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAutomation.App.Services.Implementations
{
    public class SmartCheckoutService : ISmartCheckoutService
    {
        private readonly ITransactionApiClient _apiClient;
        private readonly ILogger<SmartCheckoutService> _logger;

        public SmartCheckoutService(ITransactionApiClient apiClient, ILogger<SmartCheckoutService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<string?> CreateAsync(CreateSmartCheckoutRequest request)
        {
            const string endpointKey = "SmartCheckoutCreate";
            _logger.LogInformation("Iniciando criação de SmartCheckout/PaymentLink.");

            try
            {
                var response = await _apiClient.PostAsync<CreateSmartCheckoutRequest, CreateSmartCheckoutResponse>(endpointKey, request, default);

                if (response?.Success == true && !string.IsNullOrEmpty(response.Id))
                {
                    _logger.LogInformation("SmartCheckout criado com sucesso. ID: {SmartCheckoutId}", response.Id);
                    return response.Id;
                }
                else if(response?.Id != null)
                {
                    _logger.LogInformation("SmartCheckout criado com sucesso via Charge.ID. ID: {SmartCheckoutId}", response.Id);
                    return response.Id;
                }
                else
                {
                    string errorMessage = response?.Errors?.FirstOrDefault()?.Message ?? "A API retornou uma falha na criação do SmartCheckout.";
                    _logger.LogError("Falha ao criar SmartCheckout: {ErrorMessage}", errorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar SmartCheckout.");
                return null;
            }
        }
    }
}