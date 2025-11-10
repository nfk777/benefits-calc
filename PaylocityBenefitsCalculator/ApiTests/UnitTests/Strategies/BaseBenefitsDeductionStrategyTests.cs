using Api.Dtos.Employee;
using Api.Models;
using Api.Strategies.Implementations;
using Xunit;

namespace ApiTests.UnitTests.Strategies
{
    public class BaseBenefitsDeductionStrategyTests
    {
        public BaseBenefitsDeductionStrategyTests()
        {
            _sut = new BaseBenefitsDeductionStrategy();
        }
        public static TheoryData<PaycheckConfiguration, decimal> CalculateEmployeeBasePaycheckDeductionData =>
        new()
        {
            { SomePaycheckConfiguration, 3888.63m },
            { SomeOtherPaycheckConfiguration, 754.23m },
            { AnotherPaycheckConfiguration, 461.54m },
            { AnotherOtherPaycheckConfiguration, 369.23m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeBasePaycheckDeductionData))]
        public void CalculateEmployeeBasePaycheckDeduction_ShouldReturnExpectedDeductionValue(PaycheckConfiguration paycheckConfig, decimal expectedValue)
        {
            var actualValue = _sut.Calculate(paycheckConfig, SomeEmployeeDto);

            Assert.Equal(expectedValue, actualValue?.Deduction);
        }

        #region Variables
        BaseBenefitsDeductionStrategy _sut;
        private static readonly PaycheckConfiguration SomePaycheckConfiguration = new PaycheckConfiguration()
        {
            BaseBenefitsCost = 8425.36m,
            ChecksPerYear = 26
        };
        private static readonly PaycheckConfiguration SomeOtherPaycheckConfiguration = new PaycheckConfiguration()
        {
            BaseBenefitsCost = 754.23m,
            ChecksPerYear = 12
        };
        private static readonly PaycheckConfiguration AnotherPaycheckConfiguration = new PaycheckConfiguration()
        {
            BaseBenefitsCost = 1000.00m,
            ChecksPerYear = 26
        };
        GetEmployeeDto SomeEmployeeDto = new GetEmployeeDto();
        private static readonly PaycheckConfiguration AnotherOtherPaycheckConfiguration = new PaycheckConfiguration()
        {
            BaseBenefitsCost = 1600.00m,
            ChecksPerYear = 52
        };
        #endregion
    }
}
