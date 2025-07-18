using Api.Dtos.Employee;
using Api.Helpers;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using System.Net;

namespace Api.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private IEmployeeRepository _employeeRepo;
        public EmployeeService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<EmployeeDataResponse<List<GetEmployeeDto>>> GetAllAsync()
        {
            var responseObject = new EmployeeDataResponse<List<GetEmployeeDto>>()
            {
                EmployeeData = new List<GetEmployeeDto>()
            };

            var employees = await _employeeRepo.GetAllEmployeesAsync();

            // In the event that the collection includes an employee whose data is in a faulted state (their number of spoues/domestic partners exceeds the established maximum), I have opted to remove them from the returned collection rather than returning an error that would cause the entire collection to not be returned. 
            // As noted below, I would expect that normally client-side, server-side, and db constraints would prevent write operations from creating data in this state but if we had data in our system that pre-dates said validations we could use this approach to validate in code since our Repos are currently mocked
            // Were we to replace our MockEmployeeRepository with a real repository we could push this logic down to the query and test the query itself with an in-memory db like sqlite or the one that EF comes with
            foreach (var employee in employees) 
            { 
                if (!employee.Dependents.Any())
                {
                    responseObject.EmployeeData.Add(employee);
                }
                else if (employee.Dependents.Any() && EmployeeHelper.EmployeePartnersValid(employee.Dependents, 1))
                {
                    responseObject.EmployeeData.Add(employee);
                }
            }

            responseObject.Status = Status.Success;
            return responseObject;
        }

        public async Task<EmployeeDataResponse<GetEmployeeDto>> GetEmployeeAsync(int id)
        {
            var responseObject = new EmployeeDataResponse<GetEmployeeDto>();
            var employee = await _employeeRepo.GetEmployeeAsync(id);
            if (employee is null)
            {
                responseObject.Status = Status.NotFound;
                return responseObject;
            }

            // In the event that we have an employee whose data is in a faulted state (their number of spoues/domestic partners exceeds the established maximum) we return 500 to indicate that our data for this employee is invalid but there was no issue with the request
            // We would normally expect that data should not be in this state because validations for write operations to enforce this rule should exist on the client, server, and probably also be enforced via db constraints
            // We could enforce this at the repo level (return null if no employee with this id found that violates our constraint) but there is some value of handling it here so that we can apply business logic (what if we wanted to return a message indicating that the client data is faulted, as below?)
            if (employee.Dependents.Any() && !EmployeeHelper.EmployeePartnersValid(employee.Dependents, 1))
            {
                responseObject.Status = Status.InvalidData;
                responseObject.Message = $"Employee {employee.FirstName} {employee.LastName} has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum";
                return responseObject;
            }

            responseObject.EmployeeData = employee;
            responseObject.Status = Status.Success;
            return responseObject;
        }
    }
}
