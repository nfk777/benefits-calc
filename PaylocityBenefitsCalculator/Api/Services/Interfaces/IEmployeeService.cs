using Api.Dtos.Employee;
using Api.Models;

namespace Api.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDataResponse<List<GetEmployeeDto>>> GetAllAsync();
        Task<EmployeeDataResponse<GetEmployeeDto>> GetEmployeeAsync(int id);

    }
}
