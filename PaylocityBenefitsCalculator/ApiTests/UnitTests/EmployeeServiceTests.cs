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
using System.Net;
using System.Text;
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
            _sut = new EmployeeService(_mockEmployeeRepo.Object);
        }
        #region Tests
        [Fact]
        public async Task GetEmployee_WhenNoEmployeeFoundForId_ShouldReturnResponseObjectWithNotFoundStatusCode()
        {
            GivenNoEmployeeForId();
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusCodeIsNotFound();
        }

        [Fact]
        public async Task GetEmployee_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithInternalServerErrorStatusCode()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusCodeIsInternalServerError();
        }

        [Fact]
        public async Task GetEmployee_WhenEmployeeFoundWithInvalidPartners_ShouldReturnResponseObjectWithExpectedMessage()
        {
            GivenEmployeeWithSomeNumberOfPartners(2);
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusCodeIsExpectedErrorMessage();
        }

        [Fact]
        public async Task GetEmployee_WhenEmployeeFoundWithValidPartners_ShouldReturnResponseObjectWithEmployeeData()
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
            ThenResponseEmployeeDataIsExpectedDtoList();
        }

        [Fact]
        public async Task GetAllAsync_WhenEmployeeWithInvalidNumberOfPartnersInCollection_ShouldReturnExpectedEmployeeDtos()
        {
            GivenEmployeesIncludesEmployeeWithPartnersExceedingMaximumOf(1);
            await WhenGetEmployees();
            ThenResponseEmployeeDataIsExpectedDtoList();
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
        #endregion

        #region When
        private async Task WhenGetEmployee()
        {
            SomeGetEmployeeDataResponse = await _sut.GetEmployeeAsync(SomeEmployeeId);
        }

        private async Task WhenGetEmployees()
        {
            SomeGetEmployeesDataResponse = await _sut.GetAllAsync();
        }
        #endregion

        #region Then
        private void ThenResponseEmployeeDataStatusCodeIsNotFound()
        {
            Assert.Equal(HttpStatusCode.NotFound, SomeGetEmployeeDataResponse?.StatusCode);
        }

        private void ThenResponseEmployeeDataStatusCodeIsInternalServerError()
        {
            Assert.Equal(HttpStatusCode.InternalServerError, SomeGetEmployeeDataResponse?.StatusCode);
        }

        private void ThenResponseEmployeeDataStatusCodeIsExpectedErrorMessage()
        {
            var expectedErrorMessage = $"Employee {SomeGetEmployeeDto?.FirstName} {SomeGetEmployeeDto?.LastName} has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum";
            Assert.Equal(expectedErrorMessage, SomeGetEmployeeDataResponse?.Message);
        }

        private void ThenResponseEmployeeDataIsExpectedDto()
        {
            Assert.Equal(SomeGetEmployeeDto, SomeGetEmployeeDataResponse?.EmployeeData);
        }

        private void ThenResponseEmployeeDataIsExpectedDtoList()
        {
            Assert.Equal(SomeGetEmployeeDtos, SomeGetEmployeesDataResponse?.EmployeeData);
        }

        #endregion

        #region Variables
        private IEmployeeService _sut;
        private Mock<IEmployeeRepository> _mockEmployeeRepo;
        private Fixture _fixture;
        private GetEmployeeDto? SomeGetEmployeeDto;
        private EmployeeDataResponse<GetEmployeeDto>? SomeGetEmployeeDataResponse;
        private EmployeeDataResponse<List<GetEmployeeDto>>? SomeGetEmployeesDataResponse;
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
