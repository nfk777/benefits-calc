using Api.Models;
using Api.Repositories.Interfaces;

namespace Api.Repositories.Implementations
{
    public class MockPaycheckConfigurationRepository : IPaycheckConfigurationRepository
    {

        // Mocks async repository reads from a database, which could be managed by an ORM such as EntityFramework. These mocked calls are synchronous but I am making the contract async because the actual implementation and contract would likely be async and return a Task<T>
        // The idea of having this configuration stored in the database is that we could later associate these records with a companyId, a country code, or some other identifier such that we could retrieve different configurations for different use cases and this method could take the appropriate args to retrieve the right configuration
        // This would also be decoupled from the code in the sense that we could introduce new configurations through a db migration without necessarily having to release or deploy new code
        // As this is a mock implementation, I'm not wrapping the logic for these repo methods in try/catch blocks but I would likely do this in a real implementation especially to log exceptions and to help handle exceptions gracefully
        public async Task<PaycheckConfiguration> GetPaycheckConfigurationAsync()
        {
            return MockPaycheckConfigurations.FirstOrDefault();
        }

        private readonly List<PaycheckConfiguration> MockPaycheckConfigurations = new()
        {
            new()
            {
                BaseBenefitsCost = 1000.00m,
                MonthlyDeductionPerDependent = 600.00m,
                HighWageEarnerSalaryThreshold = 80000.00m,
                HighWageEarnerYearlyDeductionRate = 0.02m,
                DependentAgeThreshold = 50,
                AdditionalMontlyDependentAgeDeduction = 200.00m,
                ChecksPerYear = 26
            }
        };
    }
}
