namespace Api.Models
{
    public class PaycheckConfiguration
    {
        public decimal BaseBenefitsCost { get; set; }
        public decimal MonthlyDeductionPerDependent { get; set; }
        public decimal HighWageEarnerSalaryThreshold { get; set; }
        public decimal HighWageEarnerYearlyDeductionRate {  get; set; }
        public decimal AdditionalMonthlyDependentAgeDeduction { get; set; }
        public CountryCode CountryCode { get; set; }
        public int DependentAgeThreshold { get; set; }
        public int MaximumPartners { get; set; }
        public int ChecksPerYear { get; set; }
        public decimal ChecksPerMonth 
        { 
            get
            {
                return ChecksPerYear / 12.00m;
            }
        }
    }
}
