using Api.Dtos.Dependent;

namespace Api.Repositories.Interfaces
{
    public interface IDependentRepository
    {
        Task<List<GetDependentDto>> GetAllDependentsAsync();
        Task<GetDependentDto?> GetDependentAsync(int id);
    }
}
