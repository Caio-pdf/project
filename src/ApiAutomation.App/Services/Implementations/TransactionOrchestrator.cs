using ApiAutomation.App.Models;
using ApiAutomation.App.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ApiAutomation.App.Services.Implementations
{
    public class TransactionOrchestrator : ITransactionOrchestrator
    {
        private readonly ILogger<TransactionOrchestrator> _logger;
        private readonly AuthorizationService _authorizationService;
        //private readonlinly VtexService _vtexService; // Exemplo para o futuro
        //private readonly SubscriptionService _subscriptionService; // Exemplo para o futuro

        public TransactionOrchestrator(
            ILogger<TransactionOrchestrator> logger,
            AuthorizationService authorizationService)
        {
            _logger = logger;
            _authorizationService = authorizationService;
        }

        public async Task<object> ExecuteAuthorizationFlowAsync(TransactionType type, string endpointKey, Dictionary<string, object>? parameters = null)
        {
            _logger.LogInformation("Orquestrador: Iniciando fluxo de autorização para o tipo '{Type}' no endpoint '{EndpointKey}'.", type, endpointKey);

            try
            {
                _logger.LogInformation("Delegando para o AuthorizationService.AuthorizeAsync...");
                var authorizationResult = await _authorizationService.AuthorizeAsync(type, endpointKey, parameters);

                if (authorizationResult.Success)
                {
                    _logger.LogInformation("Fluxo de autorização via orquestrador concluído com SUCESSO. ChargeId: {ChargeId}", authorizationResult.Id);
                }
                else
                {
                    _logger.LogWarning("Fluxo de autorização via orquestrador concluído com FALHA. Mensagem: {Message}", authorizationResult.Message);
                }

                return authorizationResult; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado durante a orquestração do fluxo para '{Type}' e '{EndpointKey}'.", type, endpointKey);
                throw;
            }
        }

        public Task<object> ExecuteAuthorizationFlowAsync(string endpointKey)
        {
            throw new NotImplementedException();
        }
    }
}