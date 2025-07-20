namespace Api.Dtos.Employee
{
    public class GetEmployeePaycheckDto
    {
        public decimal GrossPaycheckSalary { get; set; }
        public decimal BaseBenefitsDeduction{ get; set; }
        public decimal DependentsDeduction { get; set; }
        public decimal HighWageEarnerDeduction { get; set; }
        public decimal TotalBenefitsDeduction { 
            get {
                return BaseBenefitsDeduction + DependentsDeduction + HighWageEarnerDeduction;
            }
        }
        public decimal NetPaycheckSalary { 
            get {
                return GrossPaycheckSalary - TotalBenefitsDeduction;
            } 
        }
    }
}
