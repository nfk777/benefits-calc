using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Api.Strategies.Implementations;
using System;
using System.Collections.Generic;
using Xunit;

namespace ApiTests.UnitTests.Strategies
{
    public class DependentsDeductionStrategyTests
    {
        public DependentsDeductionStrategyTests()
        {
            _sut = new DependentsDeductionStrategy();
        }

        #region Tests
        public static TheoryData<PaycheckConfiguration, decimal> CalculateEmployeeDependentPaycheckDeductionData =>
        new()
        {
            { SomePaycheckConfiguration, 553.85m },
            { SomeOtherPaycheckConfiguration, 230.77m },
            { AnotherPaycheckConfiguration, 198.97m },
            { AnotherOtherPaycheckConfiguration, 50247.12m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeDependentPaycheckDeductionData))]
        public void CalculateEmployeeDependentPaycheckDeduction_WhenAllDependentsUnderAgeThreshold_ShouldReturnDeductionRateWithNoAdditionalCost(
            PaycheckConfiguration paycheckConfig,
            decimal expectedDeduction
        )
        {
            GivenSomeDependents();
            WhenCalculate(paycheckConfig);
            ThenReturnsExpectedDeduction(expectedDeduction);
        }

        public static TheoryData<PaycheckConfiguration, decimal> CalculateEmployeeDependentPaycheckDeductionWithAgeChargeData =>
        new()
        {
            { SomePaycheckConfiguration, 1292.31m },
            { SomeOtherPaycheckConfiguration, 623.59m },
            { AnotherPaycheckConfiguration, 438.66m },
            { AnotherOtherPaycheckConfiguration, 101340.90m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeDependentPaycheckDeductionWithAgeChargeData))]
        public void CalculateEmployeeDependentPaycheckDeduction_WhenSomeDependentsAboveAgeThreshold_ShouldReturnDeductionRateWithAdditionalCosts(
            PaycheckConfiguration paycheckConfig,
            decimal expectedDeduction
        )
        {
            GivenSomeDependentsOverAgeThreshold();
            WhenCalculate(paycheckConfig);
            ThenReturnsExpectedDeduction(expectedDeduction);
        }
        #endregion

        #region Given
        private void GivenSomeDependents()
        {
            SomeDependents = new List<GetDependentDto>
            {
                new()
                {
                    Id = 2,
                    FirstName = "Child1",
                    LastName = "Morant",
                    Relationship = Relationship.Child,
                    DateOfBirth = new DateTime(2020, 6, 23)
                },
                new()
                {
                    Id = 3,
                    FirstName = "Child2",
                    LastName = "Morant",
                    Relationship = Relationship.Child,
                    DateOfBirth = new DateTime(2021, 5, 18)
                }
            };

            SomeEmployeeDto = new();
            SomeEmployeeDto.Dependents = SomeDependents;
        }

        private void GivenSomeDependentsOverAgeThreshold()
        {
            SomeDependents = new List<GetDependentDto>
            {
                new()
                {
                    Id = 2,
                    FirstName = "Child1",
                    LastName = "Morant",
                    Relationship = Relationship.Child,
                    DateOfBirth = new DateTime(2020, 6, 23)
                },
                new()
                {
                    Id = 3,
                    FirstName = "Child2",
                    LastName = "Morant",
                    Relationship = Relationship.Child,
                    DateOfBirth = new DateTime(2021, 5, 18)
                },
                 new()
                {
                    Id = 4,
                    FirstName = "DP",
                    LastName = "Jordan",
                    Relationship = Relationship.DomesticPartner,
                    DateOfBirth = new DateTime(1974, 1, 2)
                },
                new ()
                {
                    Id = 1,
                    FirstName = "Spouse",
                    LastName = "Fakename",
                    Relationship = Relationship.Spouse,
                    DateOfBirth = new DateTime(1922, 3, 3)
                }
            };

            SomeEmployeeDto = new();
            SomeEmployeeDto.Dependents = SomeDependents;
        }
        #endregion

        #region When
        private void WhenCalculate(PaycheckConfiguration paycheckConfiguration)
        {
            ActualDeductionDto = _sut.Calculate(paycheckConfiguration, SomeEmployeeDto);
        }
        #endregion

        #region Then
        private void ThenReturnsExpectedDeduction(decimal expectedDeduction)
        {
            Assert.Equal(expectedDeduction, ActualDeductionDto?.Deduction);
        }
        #endregion

        #region Variables
        private DependentsDeductionStrategy _sut;
        private List<GetDependentDto> SomeDependents = new List<GetDependentDto>();
        private DeductionDto? ActualDeductionDto;
        private GetEmployeeDto? SomeEmployeeDto;
        private static readonly PaycheckConfiguration SomePaycheckConfiguration = new PaycheckConfiguration()
        {
            MonthlyDeductionPerDependent = 600.00m,
            AdditionalMonthlyDependentAgeDeduction = 200.00m,
            DependentAgeThreshold = 50,
            ChecksPerYear = 26
        };
        private static readonly PaycheckConfiguration SomeOtherPaycheckConfiguration = new PaycheckConfiguration()
        {
            MonthlyDeductionPerDependent = 250.00m,
            AdditionalMonthlyDependentAgeDeduction = 351.12m,
            DependentAgeThreshold = 68,
            ChecksPerYear = 26
        };
        private static readonly PaycheckConfiguration AnotherPaycheckConfiguration = new PaycheckConfiguration()
        {
            MonthlyDeductionPerDependent = 431.11m,
            AdditionalMonthlyDependentAgeDeduction = 176.42m,
            DependentAgeThreshold = 51,
            ChecksPerYear = 52
        };
        private static readonly PaycheckConfiguration AnotherOtherPaycheckConfiguration = new PaycheckConfiguration()
        {
            MonthlyDeductionPerDependent = 25123.56m,
            AdditionalMonthlyDependentAgeDeduction = 423.33m,
            DependentAgeThreshold = 50,
            ChecksPerYear = 12
        };
        #endregion
    }
}
