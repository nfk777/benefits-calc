using Api.Dtos.Employee;
using Api.Helpers;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;

namespace Api.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private IEmployeeRepository _employeeRepo;
        public EmployeeService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<DataResponse<List<GetEmployeeDto>>> GetAllAsync()
        {
            // As noted in the consuming controller, this method should accept Limit and Offset pagination params
            // This method would then need to validate the pagination args, such as ensuring the submitted Limit doesn't exceed some configured maximum for example
            var responseObject = new DataResponse<List<GetEmployeeDto>>()
            {
                Data = new List<GetEmployeeDto>()
            };

            var employees = await _employeeRepo.GetAllEmployeesAsync();

            responseObject.Data = employees.ToList();
            responseObject.Status = Status.Success;
            return responseObject;
        }

        public async Task<DataResponse<GetEmployeeDto>> GetEmployeeAsync(int id)
        {
            var responseObject = new DataResponse<GetEmployeeDto>();
            var employee = await _employeeRepo.GetEmployeeAsync(id);
            if (employee is null)
            {
                responseObject.Status = Status.NotFound;
                return responseObject;
            }

            responseObject.Data = employee;
            responseObject.Status = Status.Success;
            return responseObject;
        }
    }
}
