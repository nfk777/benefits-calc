using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Api.Strategies.Interfaces;

namespace Api.Strategies.Implementations
{
    public class BaseBenefitsDeductionStrategy : IDeductionStrategy
    {
        private const string DEDUCTION_NAME = "Base Benefits Deduction";

        public DeductionType DeductionType => DeductionType.USBaseBenefits; 

        // Given that an employee has a base deduction cost of x per month, if there are more paychecks than months in a year we need to divide the deduction as evenly as possible across the projected paychecks per month
        public DeductionDto Calculate(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto)
        {
            // Round decimal math to 2 for currency
            decimal deduction = Math.Round(paycheckConfig.BaseBenefitsCost / paycheckConfig.ChecksPerMonth, 2);
            
            return new DeductionDto(DEDUCTION_NAME, deduction);
        }
    }
}
