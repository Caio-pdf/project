using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApiAutomation.Configuration;
using ApiAutomation.App.Services.Abstractions;
using ApiAutomation.App.Services.Implementations;
using ApiAutomation.App.Utilities;
using ApiAutomation.App.Services;
using System;

namespace ApiAutomation.Tests.Fixtures
{
    public class ApiFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public ApiFixture()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddDebug();
                builder.AddConsole();
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<CredentialsSettings>(Configuration.GetSection("Credentials"));
            services.Configure<AuthenticationSettings>(Configuration.GetSection("Authentication"));

            services.AddMemoryCache();
            services.AddTransient<AuthenticationHandler>();
            services.AddScoped<ITokenProvider, CachingTokenProvider>();

            services.AddScoped<ITransactionApiClient, TransactionApiClient>();
            
            services.AddHttpClient<ITransactionApiClient>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string? paymentBaseUrl = config["ApiEndpoints:Payment:BaseUrl"];
                if (string.IsNullOrEmpty(paymentBaseUrl))
                {
                    throw new InvalidOperationException("URL base 'ApiEndpoints:Payment:BaseUrl' não encontrada!");
                }
                client.BaseAddress = new Uri(paymentBaseUrl);
            })
            .AddHttpMessageHandler<AuthenticationHandler>();
            

            services.AddScoped<AuthenticationService>();
            services.AddScoped<AuthorizationService>();
            services.AddScoped<ITransactionOrchestrator, TransactionOrchestrator>();
            services.AddScoped<ITransactionRequestFactory, TransactionRequestFactory>();
            services.AddScoped<ISmartCheckoutService, SmartCheckoutService>(); 

            ServiceProvider = services.BuildServiceProvider();

            try
            {
                ServiceProvider.GetRequiredService<IOptions<CredentialsSettings>>().Value.EnsureValid();
                ServiceProvider.GetRequiredService<IOptions<AuthenticationSettings>>().Value.EnsureValid();
            }
            catch (OptionsValidationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}