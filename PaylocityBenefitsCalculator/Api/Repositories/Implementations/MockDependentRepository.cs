using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories.Interfaces;

namespace Api.Repositories.Implementations
{
    public class MockDependentRepository : IDependentRepository
    {
        // Mocks async repository reads from a database, which could be managed by an ORM such as EntityFramework. These mocked calls are synchronous but I am making the contract async because the actual implementation and contract would likely be async and return a Task<T>
        // As this is a mock, I'm not wrapping the logic for these repo methods in try/catch blocks but I would likely do this in a real implementation especially to log exceptions and to help handle exceptions gracefully
        public async Task<List<GetDependentDto>> GetAllDependentsAsync()
        {
            return MockDependents;
        }

        // The T type is marked as nullable to represent that in the case where no matching element is found, null is an acceptable return type. We will handle nulls in the consuming code.
        public async Task<GetDependentDto?> GetDependentAsync(int id)
        {
            return MockDependents.FirstOrDefault(d => d.Id == id);
        }

        private readonly List<GetDependentDto> MockDependents = new ()
        {
            new ()
            {
                Id = 1,
                FirstName = "Spouse",
                LastName = "Morant",
                Relationship = Relationship.Spouse,
                DateOfBirth = new DateTime(1998, 3, 3)
            },
            new ()
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
            }
        };
    }
}
