using Api.Dtos.Dependent;
using Api.Helpers;
using Api.Models;
using AutoFixture;
using System;
using System.Collections.Generic;
using Xunit;

namespace ApiTests.UnitTests
{
    public class EmployeeHelperTests
    {
        public EmployeeHelperTests() 
        { 
            _fixture = new Fixture();
        }

        #region Tests
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void EmployeePartnersValid_WhenEmployeePartnerCountExceedsMax_ShouldReturnFalse(int maximumPartners)
        {
            GivenSomeDependents();
            GivenSomeNumberOfPartners(maximumPartners + 1);
            GivenMaxPartnersOf(maximumPartners);
            WhenValidateEmployeePartners();
            ThenPartnersInvalid();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void EmployeePartnersValid_WhenEmployeePartnerCountDoesNotExceedMax_ShouldReturnTrue(int maximumPartners)
        {
            GivenSomeDependents();
            GivenSomeNumberOfPartners(maximumPartners);
            GivenMaxPartnersOf(maximumPartners);
            WhenValidateEmployeePartners();
            ThenPartnersValid();
        }

        [Fact]
        public void EmployeePartnersValid_WhenNoEmployeePartner_ShouldReturnTrue()
        {
            GivenSomeDependents();
            GivenMaxPartnersOf(1);
            WhenValidateEmployeePartners();
            ThenPartnersValid();
        }

        [Theory]
        [InlineData("2025-07-25", "1995-08-24", 29)]
        [InlineData("2025-07-25", "1995-07-14", 30)]
        [InlineData("2025-07-25", "2021-5-18", 4)]
        public void CalculateAge_ShouldReturnExpectedAge(string currentDateString, string birthDateString, int expectedAge)
        {
            var currentDate = DateTime.Parse(currentDateString);
            var birthDate = DateTime.Parse(birthDateString);

            var actualAge = EmployeeHelper.CalculateAge(birthDate, currentDate);

            Assert.Equal(expectedAge, actualAge);
        }

        public static TheoryData<decimal, decimal, decimal> CalculateEmployeeBasePaycheckDeductionData =>
        new()
        {
            { 8425.36m, 2.17m, 3882.65m },
            { 754.23m, 1.00m, 754.23m },
            { 1000.00m, 2.17m, 460.83m },
            { 1600.00m, 3.13m,  511.18m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeBasePaycheckDeductionData))]
        public void CalculateEmployeeBasePaycheckDeduction_ShouldReturnExpectedDeductionValue(decimal baseMonthlyBenefitDeduction, decimal paychecksPerMonth, decimal expectedValue)
        {
            var actualValue = EmployeeHelper.CalculateEmployeeBasePaycheckDeduction(baseMonthlyBenefitDeduction, paychecksPerMonth);

            Assert.Equal(expectedValue, actualValue);
        }

        public static TheoryData<decimal, decimal, int, decimal> CalculateEmployeeHighWageEarnerPaycheckDeductionData =>
        new()
        {
            { 92365.22m, 0.02m, 26, 71.05m },
            { 87211.37m, 0.13m, 26, 436.06m },
            { 138409.99m, 0.03m, 52, 79.85m  },
            { 65944.00m, 0.05m, 52, 63.41m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeHighWageEarnerPaycheckDeductionData))]

        public void CalculateEmployeeHighWageEarnerPaycheckDeduction_ShouldReturnExpectedDeductionValue(decimal yearlySalary, decimal highSalaryYearlyDeductionRate, int checksPerYear, decimal expectedValue)
        {
            var actualValue = EmployeeHelper.CalculateEmployeeHighWageEarnerPaycheckDeduction(yearlySalary, highSalaryYearlyDeductionRate, checksPerYear);

            Assert.Equal(expectedValue, actualValue);
        }

        public static TheoryData<decimal, decimal, decimal, int, decimal> CalculateEmployeeDependentPaycheckDeductionData =>
        new()
        {
            { 600.00m, 2.17m, 200.00m, 50, 553.00m },
            { 250.00m, 2.17m, 351.12m, 68, 230.41m },
            { 431.11m, 1.52m, 176.42m, 51, 567.25m },
            { 25123.56m, 3.48m, 423.33m, 51, 14438.83m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeDependentPaycheckDeductionData))]
        public void CalculateEmployeeDependentPaycheckDeduction_WhenAllDependentsUnderAgeThreshold_ShouldReturnDeductionRateWithNoAdditionalCost(
            decimal monthlyDependentCost,
            decimal payChecksPerMonth,
            decimal monthlyDependentAgeCharge,
            int dependentAgeThreshold,
            decimal expectedDeduction
        )
        {
            GivenSomeDependents();
            WhenCalculateEmployeeDependentPaycheckDeduction(
                monthlyDependentCost,
                payChecksPerMonth,
                monthlyDependentAgeCharge,
                dependentAgeThreshold
            );
            ThenReturnsExpectedDeduction(expectedDeduction);
        }

        public static TheoryData<decimal, decimal, decimal, int, decimal> CalculateEmployeeDependentPaycheckDeductionWithAgeChargeData =>
        new()
        {
            { 600.00m, 2.17m, 200.00m, 50, 1290.32m },
            { 250.00m, 2.17m, 351.12m, 68, 622.64m },
            { 431.11m, 1.52m, 176.42m, 51, 1250.57m },
            { 25123.56m, 3.48m, 423.33m, 51, 28999.30m }
        };

        [Theory]
        [MemberData(nameof(CalculateEmployeeDependentPaycheckDeductionWithAgeChargeData))]
        public void CalculateEmployeeDependentPaycheckDeduction_WhenSomeDependentsAboveAgeThreshold_ShouldReturnDeductionRateWithAdditionalCosts(
            decimal monthlyDependentCost,
            decimal payChecksPerMonth,
            decimal monthlyDependentAgeCharge,
            int dependentAgeThreshold,
            decimal expectedDeduction
        )
        {
            GivenSomeDependentsOverAgeThreshold();
            WhenCalculateEmployeeDependentPaycheckDeduction(
                monthlyDependentCost,
                payChecksPerMonth,
                monthlyDependentAgeCharge,
                dependentAgeThreshold
            );
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
        }

        private void GivenMaxPartnersOf(int max)
        {
            MaximumPartners = max;
        }

        private void GivenSomeNumberOfPartners(int numberOfPartners)
        {
            var random = new Random();
            var relationships = new[] { Relationship.Spouse, Relationship.DomesticPartner };
            // creates a number of partenrs with randomly chosen Spouse or DomesticPartner relationships so we can validate that our helper checks both
            var somePartners = _fixture
                .Build<GetDependentDto>()
                .With(x => x.Relationship, () => relationships[random.Next(relationships.Length)])
                .CreateMany(numberOfPartners);

            SomeDependents.AddRange(somePartners);
        }

        #endregion

        #region When
        private void WhenValidateEmployeePartners()
        {
            PartnersValid = EmployeeHelper.EmployeePartnersValid(SomeDependents, MaximumPartners);
        }

        private void WhenCalculateEmployeeDependentPaycheckDeduction(
            decimal monthlyDependentCost, 
            decimal paychecksPerMonth, 
            decimal monthlyDependentAgeCharge, 
            int dependentAgeThreshold
        )
        {
            ActualDeduction = EmployeeHelper.CalculateEmployeeDependentPaycheckDeduction(
                SomeDependents,
                monthlyDependentCost,
                paychecksPerMonth,
                monthlyDependentAgeCharge,
                dependentAgeThreshold,
                SomeCurrentDate
            );
        }

        #endregion

        #region Then
        private void ThenPartnersValid()
        {
            Assert.True(PartnersValid);
        }

        private void ThenPartnersInvalid()
        {
            Assert.False(PartnersValid);
        }

        private void ThenReturnsExpectedDeduction(decimal expectedDeduction)
        {
            Assert.Equal(expectedDeduction, ActualDeduction);
        }

        #endregion

        #region Variables
        private Fixture _fixture;
        private bool PartnersValid;
        private int MaximumPartners;
        private decimal ActualDeduction;
        private List<GetDependentDto> SomeDependents = new();
        private readonly DateTime SomeCurrentDate = new DateTime(2025, 7, 20);
        #endregion
    }
}
