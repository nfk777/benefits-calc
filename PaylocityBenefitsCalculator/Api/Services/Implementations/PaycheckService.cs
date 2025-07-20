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
            decimal baseDeduction = CalculateBasePaycheckDeduction(_baseBenefitsCost);
            decimal dependentDeduction = 0.00m;
            if (employee.Dependents.Any()) 
            {
                dependentDeduction += CalculateDependentPaycheckDeduction(employee.Dependents, _dependentCost);
            }

            decimal highWageEarnerDeduction = 0.00m;
            if (employee.Salary > _additionalBenefitCostThreshold)
            {
                highWageEarnerDeduction += CalculateHighWageEarnerPaycheckDeduction(employee.Salary);
            }

            employeePaycheckDto.GrossPaycheckSalary = Math.Round(employee.Salary / 26, 2);
            employeePaycheckDto.BaseBenefitsDeduction = baseDeduction;
            employeePaycheckDto.DependentsDeduction = dependentDeduction;
            employeePaycheckDto.HighWageEarnerDeduction = highWageEarnerDeduction;

            responseObject.EmployeeData = employeePaycheckDto;
            responseObject.Status = Status.Success;
            return responseObject;
        }

        private decimal CalculateBasePaycheckDeduction(decimal baseMonthlyCost)
        {
            return Math.Round(baseMonthlyCost / _payChecksPerMonth, 2);
        }

        private decimal CalculateDependentPaycheckDeduction(ICollection<GetDependentDto> dependents, decimal monthlyCost)
        {
            decimal montlyDependentDeduction = Math.Round(dependents.Count * monthlyCost, 2);

            var today = DateTime.Today;
            foreach (var dependent in dependents)
            {
                var dependentAge = EmployeeHelper.CalculateAge(dependent.DateOfBirth, today);
                if (dependentAge > _dependantAgeThreshold)
                {
                    montlyDependentDeduction += _monthlyDependentAgeCharge;
                }
            }

            decimal perPaycheckDeduction = Math.Round(montlyDependentDeduction / _payChecksPerMonth, 2);
            return perPaycheckDeduction;
        }

        private decimal CalculateHighWageEarnerPaycheckDeduction(decimal yearlySalary)
        {
            decimal yearlyDeduction = Math.Round(yearlySalary * _highSalaryYearlyCharge);
            return Math.Round(yearlyDeduction / _checksPerYear);
        }
    }
}
