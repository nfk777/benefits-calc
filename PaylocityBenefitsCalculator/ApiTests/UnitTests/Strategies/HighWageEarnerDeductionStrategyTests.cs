using Api.Dtos.Employee;
using Api.Models;
using Api.Strategies.Implementations;
using Xunit;

namespace ApiTests.UnitTests.Strategies
{
    public class HighWageEarnerDeductionStrategyTests
    {
        public HighWageEarnerDeductionStrategyTests()
        {
            _sut = new HighWageEarnerDeductionStrategy();
        }

        public static TheoryData<PaycheckConfiguration, GetEmployeeDto, decimal> CalculateEmployeeHighWageEarnerPaycheckDeductionData =>
        new()
        {
            { GivenPaycheckConfiguration(85000.00m, 0.02m, 26), GivenEmployeeWithSalary(92365.22m), 71.05m },
            { GivenPaycheckConfiguration(85000.00m, 0.13m, 26), GivenEmployeeWithSalary(87211.37m), 436.06m },
            { GivenPaycheckConfiguration(85000.00m, 0.03m, 52), GivenEmployeeWithSalary(138409.99m), 79.85m },
            { GivenPaycheckConfiguration(65000.00m, 0.05m, 52), GivenEmployeeWithSalary(65944.00m), 63.41m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeHighWageEarnerPaycheckDeductionData))]

        public void Calculate_WithSalaryAboveThreshold_ShouldReturnExpectedDeductionValue(PaycheckConfiguration paycheckConfiguration, GetEmployeeDto employeeDto, decimal expectedValue)
        {
            var actualValue = _sut.Calculate(paycheckConfiguration, employeeDto);
            Assert.Equal(expectedValue, actualValue.Deduction);
        }

        [Fact]
        public void Calculate_WithSalaryBelowThreshold_ShouldReturnZero()
        {
            var employeeDto = GivenEmployeeWithSalary(65000.00m);
            var paycheckConfig = GivenPaycheckConfiguration(85000m, 0.02m, 26);

            var actualValue = _sut.Calculate(paycheckConfig, employeeDto);

            Assert.Equal(0.00m, actualValue.Deduction);
        }

        #region Helpers
        private static GetEmployeeDto GivenEmployeeWithSalary(decimal salary)
        {
            return new GetEmployeeDto()
            {
                Salary = salary
            };
        }

        private static PaycheckConfiguration GivenPaycheckConfiguration(decimal highWageThreshold, decimal highWageRate, int checksPerYear)
        {
            return new PaycheckConfiguration()
            {
                HighWageEarnerSalaryThreshold = highWageThreshold,
                HighWageEarnerYearlyDeductionRate = highWageRate,
                ChecksPerYear = checksPerYear
            };
        }
        #endregion

        #region Variables
        private HighWageEarnerDeductionStrategy _sut;
        #endregion
    }
}
