namespace Api.Dtos.Employee
{
    public class GetEmployeePaycheckDto
    {
        public decimal GrossPaycheckSalary { get; set; }
        public decimal GrossMonthlySalary { get; set; }
        public decimal BaseBenefitsDeduction{ get; set; }
        public decimal TotalAdditionalDependentsDeduction { get; set; }
        public decimal HighWageEarnerDeduction { get; set; }
        public decimal AgeDeduction { get; set; }
        public decimal TotalBenefitsDeduction { get; set; }
    }
}
