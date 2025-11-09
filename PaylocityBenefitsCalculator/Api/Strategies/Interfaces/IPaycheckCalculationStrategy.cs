using Api.Dtos.Employee;
using Api.Models;

namespace Api.Strategies.Interfaces
{
    public interface IPaycheckCalculationStrategy
    {
        public CountryCode CountryCode { get; }
        GetEmployeePaycheckDto CalculatePaycheck(PaycheckConfiguration paycheckConfig, GetEmployeeDto employeeDto);
    }
}
