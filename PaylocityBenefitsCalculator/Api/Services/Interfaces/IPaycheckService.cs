using Api.Dtos.Employee;
using Api.Models;

namespace Api.Services.Interfaces
{
    public interface IPaycheckService
    {
        Task<EmployeeDataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int id);
    }
}
