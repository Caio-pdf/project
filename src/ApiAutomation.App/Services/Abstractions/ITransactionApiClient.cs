using System.Threading;
using System.Threading.Tasks;

namespace ApiAutomation.App.Services.Abstractions
{
    public interface ITransactionApiClient
    {
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpointPath, TRequest request, CancellationToken cancellationToken = default);

    }
}