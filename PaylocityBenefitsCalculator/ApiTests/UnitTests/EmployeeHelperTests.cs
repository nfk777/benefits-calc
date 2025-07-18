using Api.Dtos.Dependent;
using Api.Helpers;
using Api.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        #endregion

        #region Variables
        private Fixture _fixture;
        private bool PartnersValid;
        private int MaximumPartners;
        private List<GetDependentDto> SomeDependents = new();
        #endregion
    }
}
