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
        private readonly IEmployeePaycheckService _employeePaycheckService;
        public EmployeePaychecksController(IEmployeePaycheckService employeePaycheckService, IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            _employeePaycheckService = employeePaycheckService;
        }

        // @auth: Admin or Employee with the given employee id
        [SwaggerOperation(Summary = "Get paycheck for employee with id")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<GetEmployeePaycheckDto>>> GetEmployeePaycheck(int employeeId)
        {
            var dataResponse = await _employeePaycheckService.GetEmployeePaycheckAsync(employeeId);

            if (dataResponse.Status == Status.NotFound)
                return NotFound();

            if (dataResponse.Status == Status.InvalidData)
                return StatusCode(500, dataResponse.Message);

            return new ApiResponse<GetEmployeePaycheckDto>
            {
                Data = dataResponse.Data,
                Success = true
            };
        }
    }
}
