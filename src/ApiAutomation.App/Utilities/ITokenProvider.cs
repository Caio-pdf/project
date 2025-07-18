
namespace ApiAutomation.App.Utilities
{

    public interface ITokenProvider
    {
        Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default);
    }
}