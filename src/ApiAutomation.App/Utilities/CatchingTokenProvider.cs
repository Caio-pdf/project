using ApiAutomation.App.Services.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ApiAutomation.App.Utilities
{
    public class CachingTokenProvider : ITokenProvider
    {
        private readonly AuthenticationService _authService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingTokenProvider> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const string TokenCacheKey = "ApiAuthToken_v1"; 

        private readonly TimeSpan _expiryMargin = TimeSpan.FromSeconds(60); 

        public CachingTokenProvider(
            AuthenticationService authService,
            IMemoryCache cache,
            ILogger<CachingTokenProvider> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<AuthTokenDetails>(TokenCacheKey, out var cachedToken) &&
                cachedToken?.ExpiresAt > DateTimeOffset.UtcNow + _expiryMargin)
            {
                _logger.LogDebug("Token válido encontrado no cache.");
                return cachedToken.AccessToken;
            }

            _logger.LogInformation("Token ausente no cache ou expirando. Aguardando para buscar/renovar...");
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue<AuthTokenDetails>(TokenCacheKey, out cachedToken) &&
                    cachedToken?.ExpiresAt > DateTimeOffset.UtcNow + _expiryMargin)
                {
                     _logger.LogDebug("Token válido encontrado no cache após aguardar semáforo.");
                    return cachedToken.AccessToken;
                }

                _logger.LogInformation("Adquiriu lock. Buscando novo token via AuthenticationService...");

                AuthTokenDetails? newTokenDetails = await _authService.FetchNewTokenAsync(cancellationToken);

                if (newTokenDetails == null || string.IsNullOrEmpty(newTokenDetails.AccessToken))
                {
                    _logger.LogError("AuthenticationService falhou ao buscar um novo token.");
                    throw new InvalidOperationException("Não foi possível obter um token de autenticação válido.");
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(newTokenDetails.ExpiresAt);

                _cache.Set(TokenCacheKey, newTokenDetails, cacheEntryOptions);
                _logger.LogInformation("Novo token armazenado no cache. Expira em: {ExpiresAt}", newTokenDetails.ExpiresAt);

                return newTokenDetails.AccessToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}