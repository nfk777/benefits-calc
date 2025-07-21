using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.UnitTests
{
    public class PaycheckServiceTests
    {

        public PaycheckServiceTests() 
        { 
            _mockPaycheckConfigRepo = new Mock<IPaycheckConfigurationRepository>();
            _sut = new PaycheckService(_mockPaycheckConfigRepo.Object);
        }

        #region Tests
        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenPaycheckConfigNull__ShouldReturnResponseObjectWithInvalidDataStatus()
        {
            GivenNullPaycheckConfiguration();
            GivenEmployee(EmployeeWithoutDependents);
            await WhenGetEmployeePaycheckAsync();
            ThenResponseStatusIsInvalidData();
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenExceptionThrown_ShouldReturnResponseObjectWithInvalidDataStatusAndMessage()
        {
            GivenGetPaycheckConfigThrowsAnException();
            GivenEmployee(EmployeeWithoutDependents);
            await WhenGetEmployeePaycheckAsync();
            ThenResponseObjectHasExpectedMessage();
            ThenResponseStatusIsInvalidData();
        }

        public static TheoryData<GetEmployeeDto, GetEmployeePaycheckDto> GetEmployeePaycheckAsyncData =>
        new()
        {
            { EmployeeWithoutDependents, ExpectedEmployeeWithoutDependentsPaycheck },
            { EmployeeWithDependents, ExpectedEmployeeWithDependentsPaycheck },
            { EmployeeWithDependentOverAgeThreshold, ExpectedEmployeeWithDependentAboveAgeThresholdPaycheck }
        };

        [Theory]
        [MemberData(nameof(GetEmployeePaycheckAsyncData))]
        public async Task GetEmployeePaycheckAsync_WhenPaycheckConfig_ShouldReturnExpectedResposneObjectWithSuccessStatus(GetEmployeeDto employeeDto, GetEmployeePaycheckDto expectedPaycheckDto)
        {
            GivenPaycheckConfiguration();
            GivenEmployee(employeeDto);
            await WhenGetEmployeePaycheckAsync();
            ThenResponseObjectDtoHasExpectedValues(expectedPaycheckDto);
            ThenResponseStatusIsSuccess();
        }
        #endregion

        #region Given
        private void GivenNullPaycheckConfiguration()
        {
            _mockPaycheckConfigRepo.Setup(p => p.GetPaycheckConfigurationAsync()).ReturnsAsync(null as PaycheckConfiguration);
        }

        private void GivenPaycheckConfiguration()
        {
            _mockPaycheckConfigRepo.Setup(p => p.GetPaycheckConfigurationAsync()).ReturnsAsync(SomePaycheckConfiguration);
        }

        private void GivenEmployee(GetEmployeeDto employeeDto)
        {
            SomeEmployeeDto = employeeDto;
        }

        private void GivenGetPaycheckConfigThrowsAnException()
        {
            _mockPaycheckConfigRepo.Setup(p => p.GetPaycheckConfigurationAsync()).ThrowsAsync(new Exception());
        }

        #endregion

        #region When
        private async Task WhenGetEmployeePaycheckAsync()
        {
            SomeEmployeeDataResponse = await _sut.GetEmployeePaycheckAsync(SomeEmployeeDto);
        }
        #endregion

        #region Then
        private void ThenResponseStatusIsInvalidData()
        {
            Assert.Equal(Status.InvalidData, SomeEmployeeDataResponse?.Status);
        }

        private void ThenResponseObjectHasExpectedMessage()
        {
            Assert.Equal(ExpectedErrorMessage, SomeEmployeeDataResponse?.Message);
        }

        private void ThenResponseStatusIsSuccess()
        {
            Assert.Equal(Status.Success, SomeEmployeeDataResponse?.Status);
        }

        private void ThenResponseObjectDtoHasExpectedValues(GetEmployeePaycheckDto expectedDto)
        {
            GetEmployeePaycheckDto actualDto = SomeEmployeeDataResponse.EmployeeData!;
            Assert.Equal(expectedDto.GrossPaycheckSalary, actualDto.GrossPaycheckSalary);
            Assert.Equal(expectedDto.BaseBenefitsDeduction, actualDto.BaseBenefitsDeduction);
            Assert.Equal(expectedDto.DependentsDeduction, actualDto.DependentsDeduction);
            Assert.Equal(expectedDto.HighWageEarnerDeduction, actualDto.HighWageEarnerDeduction);
            Assert.Equal(expectedDto.TotalBenefitsDeduction, actualDto.TotalBenefitsDeduction);
            Assert.Equal(expectedDto.NetPaycheckSalary, actualDto.NetPaycheckSalary);
        }
        #endregion

        #region Variables
        private Mock<IPaycheckConfigurationRepository> _mockPaycheckConfigRepo;
        private EmployeeDataResponse<GetEmployeePaycheckDto> SomeEmployeeDataResponse;
        private GetEmployeeDto SomeEmployeeDto;
        private readonly string ExpectedErrorMessage = "An error occurred calculating employee paycheck, please try again later";
        private readonly PaycheckConfiguration SomePaycheckConfiguration = new()
        {
            BaseBenefitsCost = 1000.00m,
            MonthlyDeductionPerDependent = 600.00m,
            HighWageEarnerSalaryThreshold = 80000.00m,
            HighWageEarnerYearlyDeductionRate = 0.02m,
            DependentAgeThreshold = 50,
            AdditionalMontlyDependentAgeDeduction = 200.00m,
            ChecksPerYear = 26
        };
        private static readonly GetEmployeeDto EmployeeWithoutDependents = new()
        {
            Id = 1,
            FirstName = "LeBron",
            LastName = "James",
            Salary = 75420.99m,
            DateOfBirth = new DateTime(1984, 12, 30)
        };
        private static readonly GetEmployeeDto EmployeeWithDependents = new()
        {
            Id = 2,
            FirstName = "Ja",
            LastName = "Morant",
            Salary = 92365.22m,
            DateOfBirth = new DateTime(1999, 8, 10),
            Dependents = new List<GetDependentDto>
                {
                    new ()
                    {
                        Id = 1,
                        FirstName = "Spouse",
                        LastName = "Morant",
                        Relationship = Relationship.Spouse,
                        DateOfBirth = new DateTime(1998, 3, 3)
                    },
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
                }
        };
        private static readonly GetEmployeeDto EmployeeWithDependentOverAgeThreshold = new()
        {
            Id = 3,
            FirstName = "Michael",
            LastName = "Jordan",
            Salary = 143211.12m,
            DateOfBirth = new DateTime(1963, 2, 17),
            Dependents = new List<GetDependentDto>
                {
                    new()
                    {
                        Id = 4,
                        FirstName = "DP",
                        LastName = "Jordan",
                        Relationship = Relationship.DomesticPartner,
                        DateOfBirth = new DateTime(1974, 1, 2)
                    }
                }
        };
        private static readonly GetEmployeePaycheckDto ExpectedEmployeeWithoutDependentsPaycheck = new()
        {
            GrossPaycheckSalary = 2900.81m,
            BaseBenefitsDeduction = 461.54m,
            DependentsDeduction = 0.00m,
            HighWageEarnerDeduction = 0.00m
        };
        private static readonly GetEmployeePaycheckDto ExpectedEmployeeWithDependentsPaycheck = new()
        {
            GrossPaycheckSalary = 3552.51m,
            BaseBenefitsDeduction = 461.54m,
            DependentsDeduction = 830.77m,
            HighWageEarnerDeduction = 71.05m
        };
        private static readonly GetEmployeePaycheckDto ExpectedEmployeeWithDependentAboveAgeThresholdPaycheck = new()
        {
            GrossPaycheckSalary = 5508.12m,
            BaseBenefitsDeduction = 461.54m,
            DependentsDeduction = 369.23m,
            HighWageEarnerDeduction = 110.16m
        };

        private IPaycheckService _sut;
        #endregion

    }
}
