using Api.Dtos.Employee;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/Employees/{employeeId}/Paychecks")]
    public class EmployeePaychecksController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeePaychecksController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [SwaggerOperation(Summary = "Get paycheck for employee with id")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<GetEmployeePaycheckDto>>> GetEmployeePaycheck(int employeeId)
        {
            var employeeDataResponse = await _employeeService.GetEmployeePaycheckAsync(employeeId);

            if (employeeDataResponse.Status == Status.NotFound)
                return NotFound();

            if (employeeDataResponse.Status == Status.InvalidData)
                return StatusCode(500, employeeDataResponse.Message);

            return new ApiResponse<GetEmployeePaycheckDto>
            {
                Data = employeeDataResponse.EmployeeData,
                Success = true
            };
        }
    }
}
