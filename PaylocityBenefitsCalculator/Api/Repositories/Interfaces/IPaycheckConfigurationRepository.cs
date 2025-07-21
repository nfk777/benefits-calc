using Api.Models;

namespace Api.Repositories.Interfaces
{
    public interface IPaycheckConfigurationRepository
    {
        Task<PaycheckConfiguration?> GetPaycheckConfigurationAsync();
    }
}
