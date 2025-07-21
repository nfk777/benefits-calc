using Api.Dtos.Dependent;

namespace Api.Helpers
{
    public static class EmployeeHelper
    {
        // Constant for days in the year to calculate age
        private const double DAYS_PER_YEAR = 365.242199;

        public static bool EmployeePartnersValid(IEnumerable<GetDependentDto> dependents, int maxPartners)
        {
            return dependents.Where(d => d.Relationship == Models.Relationship.DomesticPartner || d.Relationship == Models.Relationship.Spouse).Count() <= maxPartners;
        }

        // Given a current date and birthdate, calculate an age based on elapsed days. Used for calculating dependent age to see if it exceed charge threshold
        public static int CalculateAge(DateTime birthDate, DateTime currentDate)
        {
            int age = (int)((currentDate - birthDate).TotalDays / DAYS_PER_YEAR);
            return age;
        }

        // Uses pure static functions below for handling paycheck math. Mathematical calculations functions that don't rely upon state are good candidates for static methods.

        // Given that an employee has a base deduction cost of x per month, if there are more paychecks than months in a year we need to divide the deduction as evenly as possible across the projected paychecks per month
        public static decimal CalculateEmployeeBasePaycheckDeduction(decimal baseMonthlyCost, decimal paychecksPerMonth)
        {
            // Round decimal math to 2 for currency
            return Math.Round(baseMonthlyCost / paychecksPerMonth, 2);
        }

        // Given for x number of dependents we incur a monthly cost of y as well as an additional cost of q for each dependent exceeding the age threshold, this count is t
        // Divide this total by projected pay checks per month
        // ((x * y) + (t * q))/ payChecksPerMonth
        public static decimal CalculateEmployeeDependentPaycheckDeduction(
            ICollection<GetDependentDto> dependents, 
            decimal monthlyDependentCost, 
            decimal paychecksPerMonth, 
            decimal monthlyDependentAgeCharge, 
            int dependentAgeThreshold, 
            DateTime currentDate
        )
        {
            // Calculate the initial monthly deduction of dependents
            decimal montlyDependentDeduction = Math.Round(dependents.Count * monthlyDependentCost, 2);

            // Add additional deduction for each dependent exceeding the age threshold
            foreach (var dependent in dependents)
            {
                var dependentAge = CalculateAge(dependent.DateOfBirth, currentDate);
                if (dependentAge > dependentAgeThreshold)
                {
                    montlyDependentDeduction += monthlyDependentAgeCharge;
                }
            }

            // Divide the total monthly dependent deduction over projected paychecks per month to determine the amount per pay check
            decimal perPaycheckDeduction = Math.Round(montlyDependentDeduction / paychecksPerMonth, 2);
            return perPaycheckDeduction;
        }

        // Given a yearly salary x an employee will incur an additional cost at a rate of y where y is a percentage of their salary
        // We take this number and divide it by the number of checks per year spreading the total deduction evenly over all paychecks
        public static decimal CalculateEmployeeHighWageEarnerPaycheckDeduction(decimal yearlySalary, decimal highSalaryYearlyDeductionRate, int checksPerYear)
        {
            decimal yearlyDeduction = Math.Round(yearlySalary * highSalaryYearlyDeductionRate, 2);
            return Math.Round(yearlyDeduction / checksPerYear, 2);
        }
    }
}
