using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Helpers;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;

namespace Api.Services.Implementations
{
    public class PaycheckService : IPaycheckService
    {
        private IEmployeeService _employeeService;
        private IPaycheckConfigurationRepository _paycheckConfigRepo;

        public PaycheckService(IEmployeeService employeeService, IPaycheckConfigurationRepository paycheckConfigRepo)
        {
            _employeeService = employeeService;
            _paycheckConfigRepo = paycheckConfigRepo;
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
            var employeeDataResponse = await _employeeService.GetEmployeeAsync(id);

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
