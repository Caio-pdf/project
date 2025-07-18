using ApiAutomation.App.Models;
using System.Threading.Tasks;

namespace ApiAutomation.App.Services.Abstractions
{
    public interface ISmartCheckoutService
    {
        Task<string?> CreateAsync(CreateSmartCheckoutRequest request);
    }
}