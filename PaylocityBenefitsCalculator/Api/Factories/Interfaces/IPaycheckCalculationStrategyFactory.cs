using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Factories.Interfaces
{
    public interface IPaycheckCalculationStrategyFactory
    {
        // In the future we could take additional arguments to further define localities, like States for the US or Provinces for Canada, as examples
        public IPaycheckCalculationStrategy? GetPaycheckCalculationStrategyByLocation(CountryCode countryCode);
    }
}
