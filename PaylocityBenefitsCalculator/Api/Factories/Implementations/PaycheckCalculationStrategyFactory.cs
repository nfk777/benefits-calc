using Api.Factories.Interfaces;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Factories.Implementations
{
    public class PaycheckCalculationStrategyFactory : IPaycheckCalculationStrategyFactory
    {
        private readonly IReadOnlyDictionary<CountryCode, IPaycheckCalculationStrategy> _paycheckCalculationLocationDictionary;
        public PaycheckCalculationStrategyFactory(IEnumerable<IPaycheckCalculationStrategy> paycheckCalculationStrategies)
        {
            // In the future we could take a more granular approach for locations such as incorporating State codes, for example but we would have to rethink our Key approach, for now having a strategy per country will work with the deductions approach we have implemented
            _paycheckCalculationLocationDictionary = paycheckCalculationStrategies.ToDictionary(s => s.CountryCode);
        }

        public IPaycheckCalculationStrategy? GetPaycheckCalculationStrategyByLocation(CountryCode countryCode)
        {
            if (_paycheckCalculationLocationDictionary.TryGetValue(countryCode, out var strategy))
            {
                return strategy;
            }

            return null;
        }
    }
}
