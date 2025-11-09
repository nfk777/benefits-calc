using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Strategies.Implementations
{
    public class HighWageEarnerDeductionStrategy : IDeductionStrategy
    {
        private const string DEDUCTION_NAME = "High Wage Earner Deduction";

        public DeductionType DeductionType => DeductionType.USHighWageEarner;

        // Given a yearly salary x an employee will incur an additional cost at a rate of y where y is a percentage of their salary
        // We take this number and divide it by the number of checks per year spreading the total deduction evenly over all paychecks
        public DeductionDto Calculate(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto)
        {
            decimal deduction = 0.00m;
            
            if (employeeDto.Salary > paycheckConfig.HighWageEarnerSalaryThreshold)
            {
                decimal yearlyDeduction = Math.Round(employeeDto.Salary * paycheckConfig.HighWageEarnerYearlyDeductionRate, 2);
                deduction = Math.Round(yearlyDeduction / paycheckConfig.ChecksPerYear, 2);
            }
            
            return new DeductionDto(DEDUCTION_NAME, deduction);
        }
    }
}
