using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ApiAutomation.App.Services.Implementations;
using ApiAutomation.Tests.Fixtures;

namespace ApiAutomation.Tests
{
    public class AuthenticationTests : IClassFixture<ApiFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly AuthenticationService _authService;

        public AuthenticationTests(ApiFixture fixture, ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _authService = fixture.ServiceProvider.GetRequiredService<AuthenticationService>();
            _outputHelper.WriteLine("AuthenticationTests: Instância criada, serviço obtido da fixture.");
        }

        [Fact]
        public async Task FetchNewTokenAsync_WithValidCredentials_ShouldReturnValidTokenDetails()
        {
            _outputHelper.WriteLine($"Iniciando teste: {nameof(FetchNewTokenAsync_WithValidCredentials_ShouldReturnValidTokenDetails)}");
            _outputHelper.WriteLine($"Tentando obter token da API em {_authService.GetServiceUrlForLogging()}");

            AuthTokenDetails? tokenDetails = null;
            Exception? exception = null;

            _outputHelper.WriteLine("Chamando FetchNewTokenAsync...");
            try
            {
                tokenDetails = await _authService.FetchNewTokenAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
                _outputHelper.WriteLine($"ERRO ao chamar FetchNewTokenAsync: {ex.GetType().Name} - {ex.Message}");
                _outputHelper.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Assert.Null(exception);

            _outputHelper.WriteLine("Assert: Verificando se tokenDetails não é nulo...");
            Assert.NotNull(tokenDetails);

            _outputHelper.WriteLine("Assert: Verificando se AccessToken não está vazio...");
            Assert.False(string.IsNullOrWhiteSpace(tokenDetails.AccessToken));

            _outputHelper.WriteLine($"Assert: Verificando se ExpiresAt ({tokenDetails.ExpiresAt}) é maior que agora...");
            Assert.True(tokenDetails.ExpiresAt > DateTimeOffset.UtcNow, "A data de expiração deve ser no futuro.");

            _outputHelper.WriteLine("Assert: Verificando se ExpiresAt é razoável (ex: não muito longe no futuro)...");
            Assert.True(tokenDetails.ExpiresAt <= DateTimeOffset.UtcNow.AddDays(1), "A data de expiração parece irrealmente longa.");

            _outputHelper.WriteLine($"SUCESSO: Token recebido: {tokenDetails.AccessToken.Substring(0, Math.Min(tokenDetails.AccessToken.Length, 20))}...");
            _outputHelper.WriteLine($"Expira em: {tokenDetails.ExpiresAt} (UTC)");
            _outputHelper.WriteLine($"Duração estimada: {tokenDetails.ExpiresAt - DateTimeOffset.UtcNow}");
        }
    }

}