using ApiAutomation.App.Models;

namespace ApiAutomation.App.Services.Abstractions
{
    public interface ITransactionRequestFactory
    {
        object CreateRequest(TransactionType type, Dictionary<string, object>? parameters = null);
    }
}