using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Factories.Interfaces;
using Api.Models;
using Api.Strategies.Implementations;
using Api.Strategies.Interfaces;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace ApiTests.UnitTests.Strategies
{
    public class USPaycheckCalculationStrategyTests
    {
        public USPaycheckCalculationStrategyTests()
        {
            _mockDeductionFactory = new Mock<IDeductionStrategyFactory>();
            _sut = new USPaycheckCalculationStrategy(_mockDeductionFactory.Object);
        }

        #region Tests
        [Fact]
        public void CalculatePaycheck_WithNoDeductions_ShouldReturnCorrectGrossAndNetPay()
        {
            GivenSomeEmployee();
            GivenSomePaycheckConfiguration();
            GivenNoDeductions();
            WhenCalculatePaycheck();
            ThenGrossPayShouldBe(2884.62m);
            ThenNetPayShouldBe(2884.62m);
            ThenTotalDeductionsShouldBe(0.00m);
            ThenDeductionsListShouldBeEmpty();
        }

        [Fact]
        public void CalculatePaycheck_WithSingleDeduction_ShouldCalculateCorrectNetPay()
        {
            GivenSomeEmployee();
            GivenSomePaycheckConfiguration();
            GivenSingleDeduction("Base Benefits Deduction", 200.00m);
            WhenCalculatePaycheck();
            ThenGrossPayShouldBe(2884.62m);
            ThenTotalDeductionsShouldBe(200m);
            ThenNetPayShouldBe(2684.62m);
            ThenDeductionsListShouldHaveCount(1);
            ThenDeductionShouldExist("Base Benefits Deduction");
        }

        [Fact]
        public void CalculatePaycheck_WithMultipleDeductions_ShouldSumDeductionsCorrectly()
        {
            GivenSomeEmployee();
            GivenSomePaycheckConfiguration();
            GivenMultipleDeductions(
                ("Base Benefits Deduction", 150.00m),
                ("High Wage Earner Deduction", 288.46m),
                ("Dependents Deduction", 432.69m)
            );
            WhenCalculatePaycheck();
            ThenGrossPayShouldBe(2884.62m);
            ThenTotalDeductionsShouldBe(871.15m);
            ThenNetPayShouldBe(2013.47m);
            ThenDeductionsListShouldHaveCount(3);
            ThenDeductionShouldExist("Base Benefits Deduction");
            ThenDeductionShouldExist("High Wage Earner Deduction");
            ThenDeductionShouldExist("Dependents Deduction");
        }

        [Fact]
        public void CalculatePaycheck_WithZeroSalary_ShouldReturnZeroAmounts()
        {
            GivenEmployeeWithZeroSalary();
            GivenSomePaycheckConfiguration();
            GivenNoDeductions();
            WhenCalculatePaycheck();
            ThenGrossPayShouldBe(0.00m);
            ThenTotalDeductionsShouldBe(0.00m);
            ThenNetPayShouldBe(0.00m);
        }

        [Fact]
        public void CalculatePaycheck_ShouldCallDeductionFactoryWithUSCountryCode()
        {
            GivenSomeEmployee();
            GivenSomePaycheckConfiguration();
            GivenNoDeductions();
            WhenCalculatePaycheck();
            ThenDeductionStrategyFactoryShouldBeCalledWithUSCountryCode();
        }
        #endregion

        #region Given
        private void GivenSomeEmployee()
        {
            SomeEmployeeDto = new()
            {
                Salary = 75000.00m,
                FirstName = "Stacey",
                LastName = "Fakename"
            };
        }

        private void GivenEmployeeWithZeroSalary()
        {
            SomeEmployeeDto = new()
            {
                Salary = 0.00m,
                FirstName = "Ricky",
                LastName = "Zerosalary"
            };
        }

        private void GivenSomePaycheckConfiguration()
        {
            SomePaycheckConfig = new()
            {
                ChecksPerYear = 26
            };
        }

        private void GivenNoDeductions()
        {
            _mockDeductionFactory
                .Setup(f => f.GetStrategiesByLocation(CountryCode.US))
                .Returns(new List<IDeductionStrategy>());
        }

        private void GivenSingleDeduction(string deductionName, decimal amount)
        {
            var mockStrategy = new Mock<IDeductionStrategy>();
            mockStrategy
                .Setup(s => s.Calculate(It.IsAny<PaycheckConfiguration>(), It.IsAny<GetEmployeeDto>()))
                .Returns(new DeductionDto(deductionName, amount));

            _mockDeductionFactory
                .Setup(f => f.GetStrategiesByLocation(CountryCode.US))
                .Returns(new List<IDeductionStrategy> { mockStrategy.Object });
        }

        private void GivenMultipleDeductions(params (string Name, decimal Amount)[] deductions)
        {
            var strategies = new List<IDeductionStrategy>();

            foreach (var (name, amount) in deductions)
            {
                var mockStrategy = new Mock<IDeductionStrategy>();
                mockStrategy
                    .Setup(s => s.Calculate(It.IsAny<PaycheckConfiguration>(), It.IsAny<GetEmployeeDto>()))
                    .Returns(new DeductionDto(name, amount));

                strategies.Add(mockStrategy.Object);
            }

            _mockDeductionFactory
                .Setup(f => f.GetStrategiesByLocation(CountryCode.US))
                .Returns(strategies);
        }
        #endregion

        #region When
        private void WhenCalculatePaycheck()
        {
            ActualPaycheckDto = _sut.CalculatePaycheck(SomePaycheckConfig, SomeEmployeeDto);
        }
        #endregion

        #region Then
        private void ThenGrossPayShouldBe(decimal expected)
        {
            Assert.Equal(expected, ActualPaycheckDto.GrossPaycheckSalary);
        }

        private void ThenNetPayShouldBe(decimal expected)
        {
            Assert.Equal(expected, ActualPaycheckDto?.NetPaycheckSalary);
        }

        private void ThenTotalDeductionsShouldBe(decimal expected)
        {
            Assert.Equal(expected, ActualPaycheckDto?.TotalBenefitsDeduction);
        }

        private void ThenDeductionsListShouldBeEmpty()
        {
            Assert.Empty(ActualPaycheckDto?.Deductions);
        }

        private void ThenDeductionsListShouldHaveCount(int expectedCount)
        {
            Assert.Equal(expectedCount, ActualPaycheckDto?.Deductions.Count);
        }

        private void ThenDeductionShouldExist(string deductionName)
        {
            Assert.Contains(ActualPaycheckDto?.Deductions, d => d.Name == deductionName);
        }

        private void ThenDeductionStrategyFactoryShouldBeCalledWithUSCountryCode()
        {
            _mockDeductionFactory.Verify(
                f => f.GetStrategiesByLocation(CountryCode.US),
                Times.Once);
        }
        #endregion

        #region Variables

        private USPaycheckCalculationStrategy _sut;
        private PaycheckConfiguration? SomePaycheckConfig;
        private GetEmployeeDto? SomeEmployeeDto;
        private readonly Mock<IDeductionStrategyFactory> _mockDeductionFactory;
        private GetEmployeePaycheckDto? ActualPaycheckDto;
        #endregion
    }
}
