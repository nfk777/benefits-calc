using Api.Dtos.Dependent;
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
        private IPaycheckConfigurationRepository _paycheckConfigRepo;
        public EmployeeService(IEmployeeRepository employeeRepo, IPaycheckConfigurationRepository paycheckConfigRepo)
        {
            _employeeRepo = employeeRepo;
            _paycheckConfigRepo = paycheckConfigRepo;
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

        public async Task<EmployeeDataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int id)
        {
            EmployeeDataResponse<GetEmployeePaycheckDto> responseObject = new();

            // Get PaycheckConfiguration object and return an invalid data status if no configuration exists as we need it to calculate paychecks
            PaycheckConfiguration paycheckConfig = await _paycheckConfigRepo.GetPaycheckConfigurationAsync();
            if (paycheckConfig is null)
            {
                responseObject.Status = Status.InvalidData;
                responseObject.Message = "Paycheck calculation cannot be completed at this time, please try again later";
                return responseObject;
            }

            // Get the employee and if the employee is invalid or not found return immediately with appropriate status and error message
            var employeeDataResponse = await GetEmployeeAsync(id);

            if (employeeDataResponse.Status != Status.Success || employeeDataResponse.EmployeeData is null)
            {
                responseObject.Status = employeeDataResponse.Status;
                responseObject.Message = !string.IsNullOrEmpty(employeeDataResponse.Message) ? employeeDataResponse.Message : string.Empty;
                return responseObject;
            }

            var employee = employeeDataResponse.EmployeeData;
            GetEmployeePaycheckDto employeePaycheckDto = new();
            employeePaycheckDto.GrossPaycheckSalary = Math.Round(employee.Salary / paycheckConfig.ChecksPerYear, 2);
            employeePaycheckDto.BaseBenefitsDeduction = EmployeeHelper.CalculateEmployeeBasePaycheckDeduction(paycheckConfig.BaseBenefitsCost, paycheckConfig.ChecksPerMonth);
            employeePaycheckDto.DependentsDeduction = GetDependentPaycheckDeduction(
                employee.Dependents,
                paycheckConfig.MonthlyDeductionPerDependent,
                paycheckConfig.AdditionalMontlyDependentAgeDeduction,
                paycheckConfig.ChecksPerMonth,
                paycheckConfig.DependentAgeThreshold
            );
            employeePaycheckDto.HighWageEarnerDeduction = GetHighWageEarnerPaycheckDeduction(
                employee.Salary,
                paycheckConfig.HighWageEarnerSalaryThreshold,
                paycheckConfig.HighWageEarnerYearlyDeductionRate,
                paycheckConfig.ChecksPerYear
            );

            responseObject.EmployeeData = employeePaycheckDto;
            responseObject.Status = Status.Success;
            return responseObject;
        }

        private decimal GetDependentPaycheckDeduction(ICollection<GetDependentDto> dependents, decimal monthlyDependentDeduction, decimal monthlyDependentAgeDeduction, decimal paychecksPerMonth, int dependentAgeThreshold)
        {
            if (dependents.Any())
            {
                return EmployeeHelper.CalculateEmployeeDependentPaycheckDeduction(dependents, monthlyDependentDeduction, paychecksPerMonth, monthlyDependentAgeDeduction, dependentAgeThreshold);
            }
            return 0.00m;
        }

        private decimal GetHighWageEarnerPaycheckDeduction(decimal yearlySalary, decimal highWageEarnerThreshold, decimal highWageEarnerYearlyDeductionRate, int checksPerYear)
        {
            if (yearlySalary > highWageEarnerThreshold)
            {
                return EmployeeHelper.CalculateEmployeeHighWageEarnerPaycheckDeduction(yearlySalary, highWageEarnerYearlyDeductionRate, checksPerYear);
            }

            return 0.00m;
        }
    }
}
