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
    }
}
