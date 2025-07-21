using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using AutoFixture;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.UnitTests
{
    public class EmployeeServiceTests
    {
        public EmployeeServiceTests()
        {
            _fixture = new Fixture();
            _mockEmployeeRepo = new Mock<IEmployeeRepository>();
            _mockPaycheckService = new Mock<IPaycheckService>();
            _sut = new EmployeeService(_mockEmployeeRepo.Object, _mockPaycheckService.Object);
        }
        #region Tests
        [Fact]
        public async Task GetEmployeeAsync_WhenNoEmployeeFoundForId_ShouldReturnResponseObjectWithNotFoundStatus()
        {
            GivenNoEmployeeForId();
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusIsNotFound();
        }

        [Fact]
        public async Task GetEmployeeAsync_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithInvalidDataStatusStatus()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusIsInvalidDataStatus();
        }

        [Fact]
        public async Task GetEmployeeAsync_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithExpectedMessage()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusIsExpectedErrorMessage();
        }

        [Fact]
        public async Task GetEmployeeAsync_WhenEmployeeFoundWithValidPartners_ShouldReturnResponseObjectWithEmployeeData()
        {
            GivenEmployeeWithSomeNumberOfPartners(1);
            await WhenGetEmployee();
            ThenResponseEmployeeDataIsExpectedDto();
        }

        [Fact]
        public async Task GetAllAsync_WhenEmployeesAvailable_ShouldReturnExpectedEmployeeDtos()
        {
            GivenSomeEmployees();
            await WhenGetEmployees();
            ThenStatusIsSuccess();
            ThenResponseEmployeeDataIsExpectedDtoList();
        }

        [Fact]
        public async Task GetAllAsync_WhenEmployeeWithInvalidNumberOfPartnersInCollection_ShouldReturnExpectedEmployeeDtos()
        {
            GivenEmployeesIncludesEmployeeWithPartnersExceedingMaximumOf(1);
            await WhenGetEmployees();
            ThenStatusIsSuccess();
            ThenResponseEmployeeDataIsExpectedDtoList();
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenNoEmployeeFoundForId_ShouldReturnResponseObjectWithNotFoundStatus()
        {
            GivenNoEmployeeForId();
            await WhenGetEmployeePaycheck();
            ThenResponseEmployeePaycheckDataStatusIsNotFound();
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithInvalidDataStatusStatus()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployeePaycheck();
            ThenResponseEmployeePaycheckDataStatusIsInvalidDataStatus();
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithExpectedMessage()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployeePaycheck();
            ThenResponseEmployeePaycheckDataStatusIsExpectedErrorMessage();
        }

        [Fact]
        public async Task GetEmployeePaycheckAsync_WhenFoundEmployeeValid_ShouldReturnResponseObjectWithExpectedDto()
        {
            GivenValidEmployeeWithPaycheck();
            await WhenGetEmployeePaycheck();
            ThenResponseEmployeePaycheckDataStatusIsExpectedDto();
            ThenPaycheckResponseIsSuccess();
        }
        #endregion

        #region Given
        private void GivenEmployeeWithSomeNumberOfPartners(int numberOfPartners)
        {
            var partners = BuildPartners(numberOfPartners);
            
            SomeGetEmployeeDto = _fixture
                .Build<GetEmployeeDto>()
                .With(x => x.Dependents, partners)
                .Create();
            
            _mockEmployeeRepo.Setup(x => x.GetEmployeeAsync(It.IsAny<int>())).ReturnsAsync(SomeGetEmployeeDto);
        }

        private void GivenNoEmployeeForId()
        {
            _mockEmployeeRepo.Setup(e => e.GetEmployeeAsync(It.IsAny<int>())).ReturnsAsync(null as GetEmployeeDto);
        }

        private void GivenSomeEmployees()
        {
            SomeGetEmployeeDtos = _fixture
                .Build<GetEmployeeDto>()
                .With(x => x.Dependents, SomeDependents)
                .CreateMany(3)
                .ToList();

            _mockEmployeeRepo.Setup(e => e.GetAllEmployeesAsync()).ReturnsAsync(SomeGetEmployeeDtos);
        }

        private void GivenEmployeesIncludesEmployeeWithPartnersExceedingMaximumOf(int maxNumberOfPartners)
        {
            SomeGetEmployeeDtos = _fixture
                .Build<GetEmployeeDto>()
                .With(x => x.Dependents, SomeDependents)
                .CreateMany(3)
                .ToList();

            var partners = BuildPartners(maxNumberOfPartners + 1);

            var invalidEmployee = _fixture
                .Build<GetEmployeeDto>()
                .With(x => x.Dependents, partners)
                .Create();

            var employeesListWithEmployeeWithTooManyPartners = new List<GetEmployeeDto>();
            employeesListWithEmployeeWithTooManyPartners.AddRange(SomeGetEmployeeDtos);
            employeesListWithEmployeeWithTooManyPartners.Add(invalidEmployee);

            _mockEmployeeRepo.Setup(e => e.GetAllEmployeesAsync()).ReturnsAsync(employeesListWithEmployeeWithTooManyPartners);
        }

        private void GivenValidEmployeeWithPaycheck()
        {
            SomeGetEmployeeDto = _fixture
               .Build<GetEmployeeDto>()
               .With(x => x.Dependents, new List<GetDependentDto>())
               .Create();

            SomeGetEmployeePaycheckResponse = new EmployeeDataResponse<GetEmployeePaycheckDto>()
            {
                Status = Status.Success,
                EmployeeData = SomeEmployeePaycheckDto
            };

            _mockEmployeeRepo.Setup(x => x.GetEmployeeAsync(It.IsAny<int>())).ReturnsAsync(SomeGetEmployeeDto);
            _mockPaycheckService.Setup(e => e.GetEmployeePaycheckAsync(SomeGetEmployeeDto)).ReturnsAsync(SomeGetEmployeePaycheckResponse);
        }
        #endregion

        #region When
        private async Task WhenGetEmployee()
        {
            SomeGetEmployeeDataResponse = await _sut.GetEmployeeAsync(SomeEmployeeId);
        }

        private async Task WhenGetEmployeePaycheck()
        {
            SomeGetEmployeePaycheckResponse = await _sut.GetEmployeePaycheckAsync(SomeEmployeeId);
        }

        private async Task WhenGetEmployees()
        {
            SomeGetEmployeesDataResponse = await _sut.GetAllAsync();
        }
        #endregion

        #region Then
        private void ThenResponseEmployeeDataStatusIsNotFound()
        {
            Assert.Equal(Status.NotFound, SomeGetEmployeeDataResponse?.Status);
        }

        private void ThenResponseEmployeeDataStatusIsInvalidDataStatus()
        {
            Assert.Equal(Status.InvalidData, SomeGetEmployeeDataResponse?.Status);
        }

        private void ThenResponseEmployeeDataStatusIsExpectedErrorMessage()
        {
            var expectedErrorMessage = $"Employee {SomeGetEmployeeDto?.FirstName} {SomeGetEmployeeDto?.LastName} has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum";
            Assert.Equal(expectedErrorMessage, SomeGetEmployeeDataResponse?.Message);
        }

        private void ThenResponseEmployeePaycheckDataStatusIsNotFound()
        {
            Assert.Equal(Status.NotFound, SomeGetEmployeePaycheckResponse?.Status);
        }

        private void ThenResponseEmployeePaycheckDataStatusIsInvalidDataStatus()
        {
            Assert.Equal(Status.InvalidData, SomeGetEmployeePaycheckResponse?.Status);
        }

        private void ThenResponseEmployeePaycheckDataStatusIsExpectedErrorMessage()
        {
            var expectedErrorMessage = $"Employee {SomeGetEmployeeDto?.FirstName} {SomeGetEmployeeDto?.LastName} has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum";
            Assert.Equal(expectedErrorMessage, SomeGetEmployeePaycheckResponse?.Message);
        }

        private void ThenResponseEmployeePaycheckDataStatusIsExpectedDto()
        {
            Assert.Equal(SomeEmployeePaycheckDto, SomeGetEmployeePaycheckResponse?.EmployeeData);
        }

        private void ThenResponseEmployeeDataIsExpectedDto()
        {
            Assert.Equal(SomeGetEmployeeDto, SomeGetEmployeeDataResponse?.EmployeeData);
        }

        private void ThenResponseEmployeeDataIsExpectedDtoList()
        {
            Assert.Equal(SomeGetEmployeeDtos, SomeGetEmployeesDataResponse?.EmployeeData);
        }

        private void ThenStatusIsSuccess()
        {
            Assert.Equal(Status.Success, SomeGetEmployeesDataResponse?.Status);
        }

        private void ThenPaycheckResponseIsSuccess()
        {
            Assert.Equal(Status.Success, SomeGetEmployeePaycheckResponse?.Status);
        }

        #endregion

        #region Variables
        private IEmployeeService _sut;
        private Mock<IEmployeeRepository> _mockEmployeeRepo;
        private Mock<IPaycheckService> _mockPaycheckService;
        private Fixture _fixture;
        private GetEmployeeDto? SomeGetEmployeeDto;
        private EmployeeDataResponse<GetEmployeeDto>? SomeGetEmployeeDataResponse;
        private EmployeeDataResponse<List<GetEmployeeDto>>? SomeGetEmployeesDataResponse;
        private EmployeeDataResponse<GetEmployeePaycheckDto>? SomeGetEmployeePaycheckResponse;
        private readonly int SomeEmployeeId = 47;
        private List<GetEmployeeDto> SomeGetEmployeeDtos = new();
        private readonly List<GetDependentDto> SomeDependents = new ()
        {
            new ()
            {
                Id = 2,
                FirstName = "Child1",
                LastName = "Morant",
                Relationship = Relationship.Child,
                DateOfBirth = new DateTime(2020, 6, 23)
            },
            new ()
            {
                Id = 3,
                FirstName = "Child2",
                LastName = "Morant",
                Relationship = Relationship.Child,
                DateOfBirth = new DateTime(2021, 5, 18)
            }
        };
        private readonly GetEmployeePaycheckDto SomeEmployeePaycheckDto = new GetEmployeePaycheckDto
        {
            GrossPaycheckSalary = 5508.12m,
            BaseBenefitsDeduction = 461.54m,
            DependentsDeduction = 369.23m,
            HighWageEarnerDeduction = 110.16m,
        };

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
