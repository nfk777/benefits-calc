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
        private IPaycheckConfigurationRepository _paycheckConfigRepo;
        public PaycheckService(IPaycheckConfigurationRepository paycheckConfigRepo) 
        {
            _paycheckConfigRepo = paycheckConfigRepo;
        }
        public async Task<EmployeeDataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(GetEmployeeDto employeeDto)
        {
            EmployeeDataResponse<GetEmployeePaycheckDto> responseObject = new();

            // Get PaycheckConfiguration object and return an invalid data status if no configuration exists as we need it to calculate paychecks
            PaycheckConfiguration? paycheckConfig = await _paycheckConfigRepo.GetPaycheckConfigurationAsync();
            if (paycheckConfig is null)
            {
                responseObject.Status = Status.InvalidData;
                responseObject.Message = "Paycheck calculation cannot be completed at this time, please try again later";
                return responseObject;
            }

            GetEmployeePaycheckDto employeePaycheckDto = new();
            employeePaycheckDto.GrossPaycheckSalary = Math.Round(employeeDto.Salary / paycheckConfig.ChecksPerYear, 2);
            employeePaycheckDto.BaseBenefitsDeduction = EmployeeHelper.CalculateEmployeeBasePaycheckDeduction(paycheckConfig.BaseBenefitsCost, paycheckConfig.ChecksPerMonth);
            employeePaycheckDto.DependentsDeduction = GetDependentPaycheckDeduction(
                employeeDto.Dependents,
                paycheckConfig.MonthlyDeductionPerDependent,
                paycheckConfig.AdditionalMontlyDependentAgeDeduction,
                paycheckConfig.ChecksPerMonth,
                paycheckConfig.DependentAgeThreshold
            );
            employeePaycheckDto.HighWageEarnerDeduction = GetHighWageEarnerPaycheckDeduction(
                employeeDto.Salary,
                paycheckConfig.HighWageEarnerSalaryThreshold,
                paycheckConfig.HighWageEarnerYearlyDeductionRate,
                paycheckConfig.ChecksPerYear
            );

            responseObject.EmployeeData = employeePaycheckDto;
            responseObject.Status = Status.Success;
            return responseObject;
        }

        private decimal GetDependentPaycheckDeduction(
            ICollection<GetDependentDto> dependents, 
            decimal monthlyDependentDeduction, 
            decimal monthlyDependentAgeDeduction, 
            decimal paychecksPerMonth, 
            int dependentAgeThreshold)
        {
            if (dependents.Any())
            {
                return EmployeeHelper.CalculateEmployeeDependentPaycheckDeduction(
                    dependents, 
                    monthlyDependentDeduction, 
                    paychecksPerMonth, 
                    monthlyDependentAgeDeduction, 
                    dependentAgeThreshold,
                    DateTime.Today
                );
            }
            return 0.00m;
        }

        private decimal GetHighWageEarnerPaycheckDeduction(
            decimal yearlySalary, 
            decimal highWageEarnerThreshold, 
            decimal highWageEarnerYearlyDeductionRate, 
            int checksPerYear
        )
        {
            if (yearlySalary > highWageEarnerThreshold)
            {
                return EmployeeHelper.CalculateEmployeeHighWageEarnerPaycheckDeduction(
                    yearlySalary, 
                    highWageEarnerYearlyDeductionRate, 
                    checksPerYear
                );
            }

            return 0.00m;
        }
    }
}
