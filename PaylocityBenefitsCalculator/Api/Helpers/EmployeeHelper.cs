using Api.Dtos.Dependent;

namespace Api.Helpers
{
    public static class EmployeeHelper
    {
        public static bool EmployeePartnersValid(IEnumerable<GetDependentDto> dependents, int maxPartners)
        {
            return dependents.Where(d => d.Relationship == Models.Relationship.DomesticPartner || d.Relationship == Models.Relationship.Spouse).Count() <= maxPartners;
        }
    }
}
