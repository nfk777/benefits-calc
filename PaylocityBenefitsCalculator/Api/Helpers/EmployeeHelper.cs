using Api.Dtos.Dependent;

namespace Api.Helpers
{
    public static class EmployeeHelper
    {
        private static readonly double DaysPerYear = 365.242199;

        public static bool EmployeePartnersValid(IEnumerable<GetDependentDto> dependents, int maxPartners)
        {
            return dependents.Where(d => d.Relationship == Models.Relationship.DomesticPartner || d.Relationship == Models.Relationship.Spouse).Count() <= maxPartners;
        }

        public static int CalculateAge(DateTime birthDate, DateTime currentDate)
        {
            int age = (int)((currentDate - birthDate).TotalDays / DaysPerYear);
            return age;
        }

        public static decimal CalculateEmployeeBasePaycheckDeduction(decimal baseMonthlyCost, decimal paychecksPerMonth)
        {
            return Math.Round(baseMonthlyCost / paychecksPerMonth, 2);
        }

        public static decimal CalculateEmployeeDependentPaycheckDeduction(
            ICollection<GetDependentDto> dependents, 
            decimal monthlyDependentCost, 
            decimal paychecksPerMonth, 
            decimal monthlyDependentAgeCharge, 
            int dependentAgeThreshold, 
            DateTime currentDate
        )
        {
            decimal montlyDependentDeduction = Math.Round(dependents.Count * monthlyDependentCost, 2);

            foreach (var dependent in dependents)
            {
                var dependentAge = CalculateAge(dependent.DateOfBirth, currentDate);
                if (dependentAge > dependentAgeThreshold)
                {
                    montlyDependentDeduction += monthlyDependentAgeCharge;
                }
            }

            decimal perPaycheckDeduction = Math.Round(montlyDependentDeduction / paychecksPerMonth, 2);
            return perPaycheckDeduction;
        }

        public static decimal CalculateEmployeeHighWageEarnerPaycheckDeduction(decimal yearlySalary, decimal highSalaryYearlyDeductionRate, int checksPerYear)
        {
            decimal yearlyDeduction = Math.Round(yearlySalary * highSalaryYearlyDeductionRate, 2);
            return Math.Round(yearlyDeduction / checksPerYear, 2);
        }
    }
}
