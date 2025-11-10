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

namespace ApiTests.UnitTests.Services
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
        public async Task GetEmployeeAsync_WhenNoEmployeeFoundForId_ShouldReturnResponseObjectWithNotFoundStatus()
        {
            GivenNoEmployeeForId();
            await WhenGetEmployee();
            ThenResponseEmployeeDataStatusIsNotFound();
        }

        [Fact]
        public async Task GetEmployeeAsync_WhenEmployeeFoundWithValidPartners_ShouldReturnResponseObjectWithEmployeeData()
        {
            GivenEmployee();
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
        public async Task GetAllAsync_WhenNoEmployeesAvailable_ReturnsSuccessWithEmptyList()
        {
            GivenNoEmployees();
            await WhenGetEmployees();
            ThenStatusIsSuccess();
            ThenResponseEmployeeDataIsExpectedDtoList();
        }
        #endregion

        #region Given
        private void GivenEmployee()
        {
            SomeGetEmployeeDto = _fixture
                .Build<GetEmployeeDto>()
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

        private void GivenNoEmployees()
        {
            SomeGetEmployeeDtos = new List<GetEmployeeDto>();
            _mockEmployeeRepo.Setup(e => e.GetAllEmployeesAsync()).ReturnsAsync(SomeGetEmployeeDtos);
        }

        #endregion

        #region When
        private async Task WhenGetEmployee()
        {
            SomeGetDataResponse = await _sut.GetEmployeeAsync(SomeEmployeeId);
        }

        private async Task WhenGetEmployees()
        {
            SomeGetEmployeesDataResponse = await _sut.GetAllAsync();
        }
        #endregion

        #region Then
        private void ThenResponseEmployeeDataStatusIsNotFound()
        {
            Assert.Equal(Status.NotFound, SomeGetDataResponse?.Status);
        }

        private void ThenResponseEmployeeDataIsExpectedDto()
        {
            Assert.Equal(SomeGetEmployeeDto, SomeGetDataResponse?.Data);
        }

        private void ThenResponseEmployeeDataIsExpectedDtoList()
        {
            Assert.Equal(SomeGetEmployeeDtos, SomeGetEmployeesDataResponse?.Data);
        }

        private void ThenStatusIsSuccess()
        {
            Assert.Equal(Status.Success, SomeGetEmployeesDataResponse?.Status);
        }
        #endregion

        #region Variables
        private IEmployeeService _sut;
        private Mock<IEmployeeRepository> _mockEmployeeRepo;
        private Fixture _fixture;
        private GetEmployeeDto? SomeGetEmployeeDto;
        private DataResponse<GetEmployeeDto>? SomeGetDataResponse;
        private DataResponse<List<GetEmployeeDto>>? SomeGetEmployeesDataResponse;
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
    }
}
