using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAutomation.App.Services.Abstractions
{
    public interface ITransactionOrchestrator
    {
        Task<object> ExecuteAuthorizationFlowAsync(TransactionType type, string endpointKey, Dictionary<string, object>? parameters = null);
    }
}