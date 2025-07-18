using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Api.Repositories.Interfaces;

namespace Api.Repositories.Implementations
{
    // Implements an interface so this mock could be swapped out for an actual implementation in the future without altering existing contracts
    public class MockEmployeeRepository : IEmployeeRepository
    {
        // Mocks async repository reads from a database, which could be managed by an ORM such as EntityFramework. These mocked calls are synchronous but I am making the contract async because the actual implementation and contract would likely be async and return a Task<T>
        // As this is a mock implementation, I'm not wrapping the logic for these repo methods in try/catch blocks but I would likely do this in a real implementation especially to log exceptions and to help handle exceptions gracefully
        public async Task<IEnumerable<GetEmployeeDto>> GetAllEmployeesAsync()
        {
            return MockEmployees;
        }

        // The T type is marked as nullable to represent that in the case where no matching element is found, null is an acceptable return type. We will handle nulls in the consuming code.
        public async Task<GetEmployeeDto?> GetEmployeeAsync(int id)
        {
            return MockEmployees.FirstOrDefault(e => e.Id == id);
        }   

        private readonly List<GetEmployeeDto> MockEmployees = new ()
        {
            new ()
            {
                Id = 1,
                FirstName = "LeBron",
                LastName = "James",
                Salary = 75420.99m,
                DateOfBirth = new DateTime(1984, 12, 30)
            },
            new ()
            {
                Id = 2,
                FirstName = "Ja",
                LastName = "Morant",
                Salary = 92365.22m,
                DateOfBirth = new DateTime(1999, 8, 10),
                // Leaving the included dependents in the GetEmployeeDto model in place because in a database this relationship would be modeled as One to Many with the Dependents table having a ForeignKey column of EmployeeId which would allow us to return an Employee with their dependents using an approprite sql query or an ORM with syntax (i.e. .Include(x => x.Dependents) in EntityFramework
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
            },
            new ()
            {
                Id = 4,
                FirstName = "Stacy",
                LastName = "Fakename",
                Salary = 100341.34m,
                DateOfBirth = new DateTime(1989, 11, 12),
                // This list of dependents violates our constraint on domestic partners/spouses
                Dependents = new List<GetDependentDto>
                {
                    new ()
                    {
                        Id = 1,
                        FirstName = "Spouse",
                        LastName = "Fakename",
                        Relationship = Relationship.Spouse,
                        DateOfBirth = new DateTime(1992, 3, 3)
                    },
                    new()
                    {
                        Id = 2,
                        FirstName = "Child1",
                        LastName = "Fakename",
                        Relationship = Relationship.Child,
                        DateOfBirth = new DateTime(2017, 6, 23)
                    },
                    new()
                    {
                        Id = 3,
                        FirstName = "Child2",
                        LastName = "Fakename",
                        Relationship = Relationship.Child,
                        DateOfBirth = new DateTime(2019, 5, 18)
                    },
                    new ()
                    {
                        Id = 6,
                        FirstName = "Domesticpartner",
                        LastName = "Fakename",
                        Relationship = Relationship.DomesticPartner,
                        DateOfBirth = new DateTime(1993, 4, 4)
                    }
                }
            },
        };
    }
}
