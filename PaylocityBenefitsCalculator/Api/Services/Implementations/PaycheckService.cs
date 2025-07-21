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
            // Uses a try/catch block for this operation to try to handle exceptions more gracefully. In the future it might be useful for the EmployerHelper methods that do the calculations to throw specific types of exceptions should they occur so we can be more certain about what part of the logic has encountered an exception
            try
            {
                // Get PaycheckConfiguration object and return an invalid data status if no configuration exists as we need it to calculate paychecks
                PaycheckConfiguration? paycheckConfig = await _paycheckConfigRepo.GetPaycheckConfigurationAsync();
                if (paycheckConfig is null)
                {
                    responseObject.Status = Status.InvalidData;
                    responseObject.Message = "Paycheck calculation cannot be completed at this time, please try again later";
                    return responseObject;
                }

                GetEmployeePaycheckDto employeePaycheckDto = new();
                // All Paycheck math uses decimal rather than float or double (or obviously int).
                // Due to the float and double utilizing binary-floating point arithmetic trying to execute precise decimal calculations can lead to rounding errors and imprecisions
                // Decimal uses base-10 arithmetic and is meant to handle decimals accurately, such as in currency
                // Decimal arithmetic is less performant, but within the requirements of this application I have chosen to favor precision
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
            catch (Exception ex)
            {
                // Could log ex or ex.Message here
                responseObject.Status = Status.InvalidData;
                responseObject.Message = "An error occurred calculating employee paycheck, please try again later";
                return responseObject;
            }
        }

        private decimal GetDependentPaycheckDeduction(
            ICollection<GetDependentDto> dependents, 
            decimal monthlyDependentDeduction, 
            decimal monthlyDependentAgeDeduction, 
            decimal paychecksPerMonth, 
            int dependentAgeThreshold)
        {
            // only execute if there are any dependents for a given employee
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
            // execute only if a given employee's salary exceeds the threshold
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
