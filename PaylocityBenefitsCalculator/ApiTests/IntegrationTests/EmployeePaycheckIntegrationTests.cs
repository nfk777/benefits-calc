using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ApiTests.IntegrationTests
{
    public class EmployeePaycheckIntegrationTests : IntegrationTest
    {
        [Fact]
        public async Task WhenAskedForAValidEmployeePaycheck_ShouldReturnCorrectEmployeePaycheck()
        {
            var response = await HttpClient.GetAsync("/api/v1/employees/3/paychecks");
            var paycheck = new GetEmployeePaycheckDto
            {
                GrossPaycheckSalary = 5508.12m,
                Deductions = new List<DeductionDto>()
            {
                new DeductionDto("Base Benefits Deduction", 461.54m),
                new DeductionDto("Dependents Deduction", 369.23m),
                new DeductionDto("High Wage Earner Deduction", 110.16m)
            },
                TotalBenefitsDeduction = 940.93m,
                NetPaycheckSalary = 4567.19m
            };

            await response.ShouldReturn(HttpStatusCode.OK, paycheck);
        }

        [Fact]
        public async Task WhenAskedForAPaycheckForNonexistentEmployee_ShouldReturn404()
        {
            var response = await HttpClient.GetAsync($"/api/v1/employees/{int.MinValue}/paychecks");
            await response.ShouldReturn(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task WhenAskedForAPaycheckForAnEmployeeWithAnInvalidNumberOfPartners_ShouldReturn500()
        {
            var response = await HttpClient.GetAsync($"/api/v1/employees/4/paychecks");
            await response.ShouldReturn(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task WhenAskedForAPaycheckForAnEmployeeWithAnInvalidNumberOfPartners_ShouldReturnExpectedMessage()
        {
            var response = await HttpClient.GetAsync($"/api/v1/employees/4/paychecks");
            await response.ShouldReturn("Employee Stacy Fakename has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum");
        }
    }
}
