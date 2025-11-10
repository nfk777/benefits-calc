using Api.Dtos.Paycheck;

namespace Api.Dtos.Employee
{
    public class GetEmployeePaycheckDto
    {
        public decimal GrossPaycheckSalary { get; set; }
        public decimal TotalBenefitsDeduction { get; set; }
        public decimal NetPaycheckSalary { get; set; }
        public List<DeductionDto> Deductions { get; set; } = new List<DeductionDto>();
    }
}
