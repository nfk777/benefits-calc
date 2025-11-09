using Api.Dtos.Employee;
using Api.Models;

namespace Api.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<DataResponse<List<GetEmployeeDto>>> GetAllAsync();
        Task<DataResponse<GetEmployeeDto>> GetEmployeeAsync(int id);
        Task<DataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int id);
    }
}
