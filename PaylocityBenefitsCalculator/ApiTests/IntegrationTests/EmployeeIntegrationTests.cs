using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Xunit;

namespace ApiTests.IntegrationTests;

public class EmployeeIntegrationTests : IntegrationTest
{
    [Fact]
    public async Task WhenAskedForAllEmployees_ShouldReturnAllEmployees()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees");
        var employees = new List<GetEmployeeDto>
        {
            new()
            {
                Id = 1,
                FirstName = "LeBron",
                LastName = "James",
                Salary = 75420.99m,
                DateOfBirth = new DateTime(1984, 12, 30)
            },
            new()
            {
                Id = 2,
                FirstName = "Ja",
                LastName = "Morant",
                Salary = 92365.22m,
                DateOfBirth = new DateTime(1999, 8, 10),
                Dependents = new List<GetDependentDto>
                {
                    new()
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
            },
            new()
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
            }
        };
        await response.ShouldReturn(HttpStatusCode.OK, employees);
    }

    [Fact]
    public async Task WhenAskedForAnEmployee_ShouldReturnCorrectEmployee()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees/1");
        var employee = new GetEmployeeDto
        {
            Id = 1,
            FirstName = "LeBron",
            LastName = "James",
            Salary = 75420.99m,
            DateOfBirth = new DateTime(1984, 12, 30)
        };
        await response.ShouldReturn(HttpStatusCode.OK, employee);
    }

    [Fact]
    public async Task WhenAskedForANonexistentEmployee_ShouldReturn404()
    {
        var response = await HttpClient.GetAsync($"/api/v1/employees/{int.MinValue}");
        await response.ShouldReturn(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenAskedForAnEmployeeWithAnInvalidNumberOfPartners_ShouldReturn500()
    {
        var response = await HttpClient.GetAsync($"/api/v1/employees/4");
        await response.ShouldReturn(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task WhenAskedForAnEmployeeWithAnInvalidNumberOfPartners_ShouldReturnExpectedMessage()
    {
        var response = await HttpClient.GetAsync($"/api/v1/employees/4");
        await response.ShouldReturn("Employee Stacy Fakename has claimed a number of spouse(s)/domestic partner(s) that exceeds the allowed maximum");
    }

    [Fact]
    public async Task WhenAskedForAValidEmployeePaycheck_ShouldReturnCorrectEmployeePaycheck()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees/3/paychecks");
        var paycheck = new GetEmployeePaycheckDto
        {
            GrossPaycheckSalary = 5508.12m,
            BaseBenefitsDeduction = 461.54m,
            DependentsDeduction = 369.23m,
            HighWageEarnerDeduction = 110.16m,
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