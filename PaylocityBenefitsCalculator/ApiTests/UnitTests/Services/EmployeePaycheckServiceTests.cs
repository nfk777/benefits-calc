using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Factories.Interfaces;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using Api.Strategies.Interfaces;
using AutoFixture;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.UnitTests.Services
{
    public class EmployeePaycheckServiceTests
    {

        public EmployeePaycheckServiceTests() 
        {
            _fixture = new Fixture();
            _mockPaycheckConfigRepo = new Mock<IPaycheckConfigurationRepository>();
            _mockEmployeeService = new Mock<IEmployeeService>();
            _mockCalculationStrategyFactory = new Mock<IPaycheckCalculationStrategyFactory>();
            _sut = new EmployeePaycheckService(_mockPaycheckConfigRepo.Object, _mockEmployeeService.Object, _mockCalculationStrategyFactory.Object);
        }

        #region Tests
        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenEmployeeNotFound_ShouldReturnResponseObjectWithInvalidDataStatus()
        {
            GivenNoEmployeeFound();
            await WhenGetEmployeePaycheckAsync();
            ThenResponseStatusIs(Status.NotFound);
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenPaycheckConfigNull__ShouldReturnResponseObjectWithInvalidDataStatus()
        {
            GivenNullPaycheckConfiguration();
            GivenEmployee(EmployeeWithoutDependents);
            await WhenGetEmployeePaycheckAsync();
            ThenResponseStatusIs(Status.InvalidData);
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenExceptionThrown_ShouldReturnResponseObjectWithInvalidDataStatusAndMessage()
        {
            GivenGetPaycheckConfigThrowsAnException();
            GivenEmployee(EmployeeWithoutDependents);
            await WhenGetEmployeePaycheckAsync();
            ThenResponseObjectHasExpectedMessage();
            ThenResponseStatusIs(Status.InvalidData);
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenEmployeePartnerDependentsExceedsConfiguredMax__ShouldReturnResponseObjectWithInvalidDataStatus()
        {
            GivenNullPaycheckConfiguration();
            GivenSomeEmployeeWithPartnersExceedingMaximum(new GetEmployeeDto());
            await WhenGetEmployeePaycheckAsync();
            ThenResponseStatusIs(Status.InvalidData);
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenRequestsPaycheckCalculationStrategy_UsesCorrectCountryCode()
        {
            GivenPaycheckConfiguration();
            GivenEmployee(EmployeeWithoutDependents);
            GivenSomePaycheckCalculationStrategy();
            await WhenGetEmployeePaycheckAsync();
            ThenCalculatePaycheckStrategyFactoryShouldBeCalledWithCountryCode(SomePaycheckConfiguration.CountryCode);
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenPaycheckCalculationStrategyNotFound_UsesCorrectCountryCode()
        {
            GivenPaycheckConfiguration();
            GivenEmployee(EmployeeWithoutDependents);
            GivenPaycheckCalculationStrategyNotFound();
            await WhenGetEmployeePaycheckAsync();
            ThenCalculatePaycheckStrategyFactoryShouldBeCalledWithCountryCode(SomePaycheckConfiguration.CountryCode);
            ThenResponseStatusIs(Status.InvalidData);
        }
        
        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenPaycheckCalculatedByStrategySuccessfully_ReturnsSuccessStatusAndPaycheckObject()
        {
            GivenPaycheckConfiguration();
            GivenEmployee(EmployeeWithoutDependents);
            GivenSomePaycheckCalculationStrategy();
            await WhenGetEmployeePaycheckAsync();
            ThenResponseStatusIs(Status.Success);
            ThenResponseDataIsExpectedPaycheckDto();
        }
        #endregion

        #region Given
        private void GivenNoEmployeeFound()
        {
            _mockEmployeeService.Setup(x => x.GetEmployeeAsync(It.IsAny<int>())).ReturnsAsync(new DataResponse<GetEmployeeDto>()
            {
                Data = null,
                Status = Status.NotFound
            });
        }

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
            _mockEmployeeService.Setup(x => x.GetEmployeeAsync(It.IsAny<int>())).ReturnsAsync(new DataResponse<GetEmployeeDto>()
            {
                Data = employeeDto,
                Status = Status.Success
            });
            SomeEmployeeDto = employeeDto;
        }

        private void GivenGetPaycheckConfigThrowsAnException()
        {
            _mockPaycheckConfigRepo.Setup(p => p.GetPaycheckConfigurationAsync()).ThrowsAsync(new Exception());
        }

        private void GivenSomeEmployeeWithPartnersExceedingMaximum(GetEmployeeDto employeeDto)
        {
            SomeEmployeeDto = employeeDto;
            SomeEmployeeDto.Dependents = BuildPartners(3);
        }

        private void GivenSomePaycheckCalculationStrategy()
        {
            var mockStrategy = new Mock<IPaycheckCalculationStrategy>();
            mockStrategy
                .Setup(x => x.CalculatePaycheck(It.IsAny<PaycheckConfiguration>(), It.IsAny<GetEmployeeDto>()))
                .Returns(SomePaycheckDto);

            _mockCalculationStrategyFactory.Setup(x => x.GetPaycheckCalculationStrategyByLocation(It.IsAny<CountryCode>()))
                .Returns(mockStrategy.Object);
        }

        private void GivenPaycheckCalculationStrategyNotFound()
        {
            _mockCalculationStrategyFactory.Setup(x => x.GetPaycheckCalculationStrategyByLocation(It.IsAny<CountryCode>()))
                .Returns((IPaycheckCalculationStrategy?)null);
        }
        #endregion

        #region When
        private async Task WhenGetEmployeePaycheckAsync()
        {
            SomeDataResponse = await _sut.GetEmployeePaycheckAsync(1);
        }
        #endregion

        #region Then
        private void ThenResponseStatusIs(Status expectedStatus)
        {
            Assert.Equal(expectedStatus, SomeDataResponse?.Status);
        }

        private void ThenResponseObjectHasExpectedMessage()
        {
            Assert.Equal(ExpectedErrorMessage, SomeDataResponse?.Message);
        }

        private void ThenCalculatePaycheckStrategyFactoryShouldBeCalledWithCountryCode(CountryCode expectedCountryCode)
        {
            _mockCalculationStrategyFactory.Verify(
                f => f.GetPaycheckCalculationStrategyByLocation(expectedCountryCode),
                Times.Once);
        }

        private void ThenResponseDataIsExpectedPaycheckDto()
        {
            Assert.Equal(SomePaycheckDto, SomeDataResponse?.Data);
        }
        #endregion

        #region Variables
        private Fixture _fixture;
        private Mock<IPaycheckConfigurationRepository> _mockPaycheckConfigRepo;
        private Mock<IEmployeeService> _mockEmployeeService;
        private Mock<IPaycheckCalculationStrategyFactory> _mockCalculationStrategyFactory;
        private DataResponse<GetEmployeePaycheckDto>? SomeDataResponse;
        private GetEmployeeDto? SomeEmployeeDto;
        private readonly string ExpectedErrorMessage = "An error occurred calculating employee paycheck, please try again later";
        private readonly PaycheckConfiguration SomePaycheckConfiguration = new()
        {
            BaseBenefitsCost = 1000.00m,
            MonthlyDeductionPerDependent = 600.00m,
            HighWageEarnerSalaryThreshold = 80000.00m,
            HighWageEarnerYearlyDeductionRate = 0.02m,
            DependentAgeThreshold = 50,
            AdditionalMonthlyDependentAgeDeduction = 200.00m,
            ChecksPerYear = 26,
            MaximumPartners = 1,
            CountryCode = CountryCode.US
        };

        private GetEmployeePaycheckDto SomePaycheckDto = new()
        {
            GrossPaycheckSalary = 4000.00m,
            Deductions = new List<DeductionDto>()
            {
                new DeductionDto("Base Benefits Deduction", 1000.00m),
                new DeductionDto("Dependents Deduction", 1200.00m)
            },
            TotalBenefitsDeduction = 2200.00m,
            NetPaycheckSalary = 1800.00m
        };

        private static readonly GetEmployeeDto EmployeeWithoutDependents = new()
        {
            Id = 1,
            FirstName = "LeBron",
            LastName = "James",
            Salary = 75420.99m,
            DateOfBirth = new DateTime(1984, 12, 30)
        };

        private IEmployeePaycheckService _sut;
        #endregion

        #region Helpers
        private List<GetDependentDto> BuildPartners(int numberOfPartners)
        {
            var random = new Random();
            var relationships = new[] { Relationship.Spouse, Relationship.DomesticPartner };
            // creates a number of partenrs with randomly chosen Spouse or DomesticPartner relationships so we can validate that our helper checks both
            var partners = _fixture
                .Build<GetDependentDto>()
                .With(x => x.Relationship, () => relationships[random.Next(relationships.Length)])
                .CreateMany(numberOfPartners);
            return partners.ToList();
        }
        #endregion
    }
}
