using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Factories.Interfaces
{
    public interface IDeductionStrategyFactory
    {
        // In the future we could take additional arguments to further define localities, like States for the US or Provinces for Canada, as examples
        IEnumerable<IDeductionStrategy> GetStrategiesByLocation(CountryCode countryCode);
    }
}
