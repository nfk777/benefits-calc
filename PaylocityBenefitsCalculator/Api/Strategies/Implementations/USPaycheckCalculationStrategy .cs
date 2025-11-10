using Api.Dtos.Employee;
using Api.Factories.Interfaces;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Strategies.Implementations
{
    public class USPaycheckCalculationStrategy : IPaycheckCalculationStrategy
    {
        private IDeductionStrategyFactory _deductionStrategyFactory;
        public CountryCode CountryCode => CountryCode.US;

        public USPaycheckCalculationStrategy(IDeductionStrategyFactory deductionStrategyFactory)
        {
            _deductionStrategyFactory = deductionStrategyFactory;
        }

        public GetEmployeePaycheckDto CalculatePaycheck(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto)
        {
            GetEmployeePaycheckDto employeePaycheckDto = new();

            // All Paycheck math uses decimal rather than float or double (or obviously int).
            // Due to the float and double utilizing binary-floating point arithmetic trying to execute precise decimal calculations can lead to rounding errors and imprecisions
            // Decimal uses base-10 arithmetic and is meant to handle decimals accurately, such as in currency
            // Decimal arithmetic is less performant, but within the requirements of this application I have chosen to favor precision
            employeePaycheckDto.GrossPaycheckSalary = Math.Round(employeeDto.Salary / paycheckConfig.ChecksPerYear, 2, MidpointRounding.ToEven);

            var deductionStrategies = _deductionStrategyFactory.GetStrategiesByLocation(CountryCode);

            foreach (IDeductionStrategy strategy in deductionStrategies)
            {
                var deductionDto = strategy.Calculate(paycheckConfig, employeeDto);
                employeePaycheckDto.Deductions.Add(deductionDto);
            }

            employeePaycheckDto.TotalBenefitsDeduction = Math.Round(employeePaycheckDto.Deductions.Sum(x => x.Deduction), 2, MidpointRounding.ToEven);
            employeePaycheckDto.NetPaycheckSalary = employeePaycheckDto.GrossPaycheckSalary - employeePaycheckDto.TotalBenefitsDeduction;

            return employeePaycheckDto;
        }
    }
}
