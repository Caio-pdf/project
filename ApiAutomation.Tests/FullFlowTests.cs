using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ApiAutomation.App.Services;
using ApiAutomation.App.Services.Abstractions;
using ApiAutomation.App.Models;
using ApiAutomation.Tests.Fixtures;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace ApiAutomation.Tests.f
{
    public class FullFlowTests : IClassFixture<ApiFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ISmartCheckoutService _smartCheckoutService;
        private readonly AuthorizationService _authorizationService;

        public FullFlowTests(ApiFixture fixture, ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _smartCheckoutService = fixture.ServiceProvider.GetRequiredService<ISmartCheckoutService>();
            _authorizationService = fixture.ServiceProvider.GetRequiredService<AuthorizationService>(); 
        }

        [Theory]
        [InlineData(TransactionType.SmartCheckoutAuthorization)] 
        [InlineData(TransactionType.PaymentLinkAuthorization)]
        public async Task FullPaymentFlow_WithGeneratedId_ShouldSucceed(TransactionType transactionType)
        {
            _outputHelper.WriteLine($"--- INICIANDO TESTE DE FLUXO COMPLETO PARA: {transactionType} ---");
            
            _outputHelper.WriteLine("Passo 1: Criando o link para obter o ID...");
            var createRequest = new CreateSmartCheckoutRequest
            {
                Amount = 1500,
                Description = $"Teste de fluxo completo para {transactionType}",
                EmailNotification = "teste@aditum.com.br"
            };
            
            string? smartCheckoutId = await _smartCheckoutService.CreateAsync(createRequest);
            
            Assert.False(string.IsNullOrEmpty(smartCheckoutId), "A criação do SmartCheckout falhou, não foi possível obter o ID.");
            _outputHelper.WriteLine($"SUCESSO no Passo 1! ID Gerado: {smartCheckoutId}");


            _outputHelper.WriteLine($"Passo 2: Executando a autorização do tipo '{transactionType}' com o ID gerado...");
            var authEndpointKey = "AuthDefault";
            var authParameters = new Dictionary<string, object>
            {
                { "smartCheckoutId", smartCheckoutId! }
            };
            
            ChargeResult? authResult = await _authorizationService.AuthorizeAsync(transactionType, authEndpointKey, authParameters);
            
            Assert.NotNull(authResult);
            _outputHelper.WriteLine($"Resultado da Autorização: Success={authResult.Success}, Msg='{authResult.Message}', Id='{authResult.Id}'");
            Assert.True(authResult.Success, $"A autorização do tipo '{transactionType}' falhou. Mensagem: {authResult.Message}");
            Assert.False(string.IsNullOrWhiteSpace(authResult.Id), "O Id da transação final não pode ser vazio.");
            _outputHelper.WriteLine($"--- SUCESSO NO FLUXO COMPLETO PARA: {transactionType}! ChargeId Final: {authResult.Id} ---");
        }
    }
}