using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Helpers;
using Api.Models;
using Api.Services.Interfaces;

namespace Api.Services.Implementations
{
    public class PaycheckService : IPaycheckService
    {
        private IEmployeeService _employeeService;

        private readonly decimal _baseBenefitsCost = 1000.00m;
        private readonly decimal _dependentCost = 600.00m;
        private readonly decimal _additionalBenefitCostThreshold = 80000.00m;
        private readonly decimal _highSalaryYearlyCharge = 0.02m;
        private readonly decimal _payChecksPerMonth = 2.17m;
        private readonly int _dependantAgeThreshold = 50;
        private readonly decimal _monthlyDependentAgeCharge = 200.00m;
        private readonly int _checksPerYear = 26;

        public PaycheckService(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<EmployeeDataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int id)
        {
            EmployeeDataResponse<GetEmployeePaycheckDto> responseObject = new();
            var employeeDataResponse = await _employeeService.GetEmployeeAsync(id);

            if (employeeDataResponse.Status != Status.Success || employeeDataResponse.EmployeeData is null)
            {
                responseObject.Status = employeeDataResponse.Status;
                responseObject.Message = !string.IsNullOrEmpty(employeeDataResponse.Message) ? employeeDataResponse.Message : string.Empty;
                return responseObject;
            }

            var employee = employeeDataResponse.EmployeeData;
            GetEmployeePaycheckDto employeePaycheckDto = new();
            employeePaycheckDto.GrossPaycheckSalary = Math.Round(employee.Salary / 26, 2);
            employeePaycheckDto.BaseBenefitsDeduction = EmployeeHelper.CalculateEmployeeBasePaycheckDeduction(_baseBenefitsCost, _payChecksPerMonth);
            employeePaycheckDto.DependentsDeduction = GetDependentPaycheckDeduction(employee.Dependents);
            employeePaycheckDto.HighWageEarnerDeduction = GetHighWageEarnerPaycheckDeduction(employee.Salary);

            responseObject.EmployeeData = employeePaycheckDto;
            responseObject.Status = Status.Success;
            return responseObject;
        }

        private decimal GetDependentPaycheckDeduction(ICollection<GetDependentDto> dependents)
        {
            if (dependents.Any())
            {
                return EmployeeHelper.CalculateEmployeeDependentPaycheckDeduction(dependents, _dependentCost, _payChecksPerMonth, _monthlyDependentAgeCharge, _dependantAgeThreshold);
            }
            return 0.00m;
        }

        private decimal GetHighWageEarnerPaycheckDeduction(decimal yearlySalary)
        {
            if (yearlySalary > _additionalBenefitCostThreshold)
            {
                return EmployeeHelper.CalculateEmployeeHighWageEarnerPaycheckDeduction(yearlySalary, _highSalaryYearlyCharge, _checksPerYear);
            }

            return 0.00m;
        }
    }
}
