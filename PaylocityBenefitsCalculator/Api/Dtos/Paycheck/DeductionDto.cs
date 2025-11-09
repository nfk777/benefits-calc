namespace Api.Dtos.Paycheck
{
    public class DeductionDto
    {
        public DeductionDto(string name, decimal deduction)
        {
            Name = name;
            Deduction = deduction;
        }

        public string Name { get; }
        public decimal Deduction { get; }
    }
}
