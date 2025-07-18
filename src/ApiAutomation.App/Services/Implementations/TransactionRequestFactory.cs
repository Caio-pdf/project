using ApiAutomation.App.Models;
using ApiAutomation.App.Services.Abstractions;
using System;
using System.Collections.Generic;

namespace ApiAutomation.App.Services.Implementations
{
    public class TransactionRequestFactory : ITransactionRequestFactory
    {
        // O método que faz a mágica acontecer.
        public object CreateRequest(TransactionType type, Dictionary<string, object>? parameters = null)
        {
            // O switch é perfeito para selecionar a lógica baseada no enum.
            return type switch
            {
                TransactionType.EcommerceAuthorization => BuildEcommerceRequest(parameters),
                TransactionType.SmartCheckoutAuthorization => BuildSmartCheckoutRequest(parameters),
                
                // Adicione outros cases aqui no futuro
                
                // Uma exceção para casos não implementados. Garante que não esqueceremos de nada.
                _ => throw new NotImplementedException($"A criação de requisição para o tipo '{type}' não foi implementada.")
            };
        }

        // Método privado para construir a requisição de E-commerce.
        private ChargeRequest<EcommerceChargePayload> BuildEcommerceRequest(Dictionary<string, object>? p)
        {
            // Aqui podemos usar "Fakers" ou dados padrão para preencher a requisição.
            // O dicionário 'p' (parameters) nos permite sobrescrever valores padrão se necessário.
            var payload = new EcommerceChargePayload
            {
                MerchantChargeId = Guid.NewGuid().ToString(),
                LateCapture = false,
                Customer = new CustomerDetails { Name = "Cliente E-commerce Teste" /* ... */ },
                Transactions = new List<TransactionDetails> { /* ... */ }
            };

            return new ChargeRequest<EcommerceChargePayload>(payload);
        }

        // Método privado para construir a requisição de SmartCheckout.
        private ChargeRequest<SmartCheckoutChargePayload> BuildSmartCheckoutRequest(Dictionary<string, object>? p)
        {
            // Validação crucial: este tipo de request PRECISA de um smartCheckoutId.
            if (p == null || !p.ContainsKey("smartCheckoutId") || p["smartCheckoutId"] is not string)
            {
                throw new ArgumentException("O parâmetro 'smartCheckoutId' é obrigatório para SmartCheckout.");
            }
            
            var payload = new SmartCheckoutChargePayload
            {
                SmartCheckoutId = (string)p["smartCheckoutId"], // Pegamos o valor dos parâmetros!
                MerchantChargeId = Guid.NewGuid().ToString(),
                Customer = new CustomerDetails { Name = "Cliente SmartCheckout Teste" /* ... */ },
                Transactions = new List<TransactionDetails> { /* ... */ }
            };

            return new ChargeRequest<SmartCheckoutChargePayload>(payload);
        }
    }
}