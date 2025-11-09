using Api.Dtos.Employee;
using Api.Models;

namespace Api.Services.Interfaces
{
    public interface IEmployeePaycheckService
    {
        Task<DataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int employeeId);
    }
}
