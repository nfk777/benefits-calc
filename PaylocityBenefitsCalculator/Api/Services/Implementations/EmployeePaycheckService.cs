using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Factories.Interfaces;
using Api.Helpers;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Strategies.Interfaces;

namespace Api.Services.Implementations
{
    public class EmployeePaycheckService : IEmployeePaycheckService
    {
        private readonly IPaycheckConfigurationRepository _paycheckConfigRepo;
        private readonly IEmployeeService _employeeReadService;
        private readonly IPaycheckCalculationStrategyFactory _paycheckCalculationStrategyFactory;

        public EmployeePaycheckService(
            IPaycheckConfigurationRepository paycheckConfigRepo, 
            IEmployeeService employeeReadService,
            IPaycheckCalculationStrategyFactory paycheckCalculationStrategyFactory
            ) 
        {
            _paycheckConfigRepo = paycheckConfigRepo;
            _employeeReadService = employeeReadService;
            _paycheckCalculationStrategyFactory = paycheckCalculationStrategyFactory;
        }
        public async Task<DataResponse<GetEmployeePaycheckDto>> GetEmployeePaycheckAsync(int employeeId)
        {
            DataResponse<GetEmployeePaycheckDto> responseObject = new();
            // Uses a try/catch block for this operation to try to handle exceptions more gracefully. In the future it might be useful for the EmployerHelper methods that do the calculations to throw specific types of exceptions should they occur so we can be more certain about what part of the logic has encountered an exception
            try
            {
                // Get employee dto and return not found data status if no employee found for id
                var getEmployeeResponse = await _employeeReadService.GetEmployeeAsync(employeeId);
                if (getEmployeeResponse.Status is not Status.Success || getEmployeeResponse.Data is null)
                {
                    responseObject.Status = Status.NotFound;
                    responseObject.Message = "No employee found";
                    return responseObject;
                }

                // Get PaycheckConfiguration object (in the future this would take arguments needed to identify the configuration, like a company and employee id) and return an invalid data status if no configuration exists as we need it to calculate paychecks
                PaycheckConfiguration? paycheckConfig = await _paycheckConfigRepo.GetPaycheckConfigurationAsync();
                if (paycheckConfig is null)
                {
                    responseObject.Status = Status.InvalidData;
                    responseObject.Message = "Paycheck calculation cannot be completed at this time, please try again later";
                    return responseObject;
                }

                var employeeDto = getEmployeeResponse.Data;
                if (employeeDto.Dependents.Any() && !EmployeeHelper.EmployeePartnersValid(employeeDto.Dependents, paycheckConfig.MaximumPartners))
                {
                    responseObject.Status = Status.InvalidData;
                    responseObject.Message = $"Employee has exceeded the maximum number of partners: {paycheckConfig.MaximumPartners}";
                    return responseObject;
                }

                // Get calculate paycheck strategy by location
                IPaycheckCalculationStrategy? paycheckCalculationStrategy = _paycheckCalculationStrategyFactory.GetPaycheckCalculationStrategyByLocation(paycheckConfig.CountryCode);
                if (paycheckCalculationStrategy is null)
                {
                    responseObject.Status = Status.InvalidData;
                    responseObject.Message = "Paycheck calculation cannot be completed at this time, please try again later";
                    return responseObject;
                }

                responseObject.Data = paycheckCalculationStrategy.CalculatePaycheck(paycheckConfig, getEmployeeResponse.Data);
                responseObject.Status = Status.Success;
                return responseObject;
            }
            catch (Exception ex)
            {
                // Could log ex or ex.Message here
                responseObject.Status = Status.InvalidData;
                responseObject.Message = "An error occurred calculating employee paycheck, please try again later";
                return responseObject;
            }
        }
    }
}
