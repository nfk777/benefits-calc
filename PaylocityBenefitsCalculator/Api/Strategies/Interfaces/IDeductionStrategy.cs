using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;

namespace Api.Strategies.Interfaces
{
    public interface IDeductionStrategy
    {
        DeductionType DeductionType { get; }
        DeductionDto Calculate(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto);
    }
}
