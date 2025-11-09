using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Helpers;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Strategies.Implementations
{
    public class DependentsDeductionStrategy : IDeductionStrategy
    {
        private const string DEDUCTION_NAME = "Dependents Deduction";

        public DeductionType DeductionType => DeductionType.USDependents;

        // Given for x number of dependents we incur a monthly cost of y as well as an additional cost of q for each dependent exceeding the age threshold, this count is t
        // Divide this total by projected pay checks per month
        // ((x * y) + (t * q))/ payChecksPerMonth
        public DeductionDto Calculate(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto)
        {
            decimal deduction = 0.00m;
            var dependents = employeeDto.Dependents;

            if (dependents.Any())
            {
                // Calculate the initial monthly deduction of dependents
                decimal monthlyDependentDeduction = Math.Round(dependents.Count * paycheckConfig.MonthlyDeductionPerDependent, 2);

                // Add additional deduction for each dependent exceeding the age threshold
                foreach (var dependent in dependents)
                {
                    var dependentAge = EmployeeHelper.CalculateAge(dependent.DateOfBirth, DateTime.Today);
                    if (dependentAge > paycheckConfig.DependentAgeThreshold)
                    {
                        monthlyDependentDeduction += paycheckConfig.AdditionalMonthlyDependentAgeDeduction;
                    }
                }

                // Divide the total monthly dependent deduction over projected paychecks per month to determine the amount per pay check
                deduction = Math.Round(monthlyDependentDeduction / paycheckConfig.ChecksPerMonth, 2);
            }

            return new DeductionDto(DEDUCTION_NAME, deduction);
        }
    }
}
