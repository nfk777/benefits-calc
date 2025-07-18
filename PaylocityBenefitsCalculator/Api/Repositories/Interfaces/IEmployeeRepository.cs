using Api.Dtos.Employee;

namespace Api.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<GetEmployeeDto>> GetAllEmployeesAsync();
        Task<GetEmployeeDto?> GetEmployeeAsync(int id);
    }
}
