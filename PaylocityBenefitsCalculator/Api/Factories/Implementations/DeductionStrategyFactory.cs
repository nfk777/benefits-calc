using Api.Factories.Interfaces;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Factories.Implementations
{
    public class DeductionStrategyFactory : IDeductionStrategyFactory
    {
        private readonly IReadOnlyDictionary<DeductionType, IDeductionStrategy> _deductionStrategyTypeDictionary;

        public DeductionStrategyFactory(IEnumerable<IDeductionStrategy> allDeductionStrategies)
        {
            _deductionStrategyTypeDictionary = allDeductionStrategies.ToDictionary(s => s.DeductionType);
        }

        public IEnumerable<IDeductionStrategy> GetStrategiesByLocation(CountryCode countryCode)
        {
            if (countryCode is CountryCode.US)
                return GetUSDeductionStrategies();

            return new List<IDeductionStrategy>();
        }

        // In the future this could take a state code argument, just as a GetCanadianDeductionStrategies could take a province code argument
        private IEnumerable<IDeductionStrategy> GetUSDeductionStrategies()
        {
            var deductionStrategies = new List<IDeductionStrategy>();

            AddDeductionStrategyIfExists(deductionStrategies, DeductionType.USBaseBenefits);
            AddDeductionStrategyIfExists(deductionStrategies, DeductionType.USDependents);
            AddDeductionStrategyIfExists(deductionStrategies, DeductionType.USHighWageEarner);

            return deductionStrategies;
        }

        private void AddDeductionStrategyIfExists(List<IDeductionStrategy> deductionStrategies, DeductionType deductionType)
        {
            if (_deductionStrategyTypeDictionary.TryGetValue(deductionType, out var strategy))
            {
                deductionStrategies.Add(strategy);
            }
        }
    }
}
