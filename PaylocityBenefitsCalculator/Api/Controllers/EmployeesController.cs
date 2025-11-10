using Api.Dtos.Employee;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // @auth: Admin
    [SwaggerOperation(Summary = "Get all employees")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetEmployeeDto>>>> GetAll()
    {
        // This endpoint should be paginated with values for "Limit" and "Offset" from FromQuery string params
        // These params should have default values such that we don't return the entire result set if they are left empty
        var dataResponse = await _employeeService.GetAllAsync();

        var result = new ApiResponse<List<GetEmployeeDto>>
        {
            Data = dataResponse.Data,
            Success = true
        };

        return result;
    }

    // @auth: Admin or Employee with Id (self)
    [SwaggerOperation(Summary = "Get employee by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetEmployeeDto>>> Get(int id)
    {
        var dataResponse = await _employeeService.GetEmployeeAsync(id);

        if (dataResponse.Status == Status.NotFound)
            return NotFound();

        if (dataResponse.Status == Status.InvalidData)
            return StatusCode(500, dataResponse.Message);

        return new ApiResponse<GetEmployeeDto>
        {
            Data = dataResponse.Data,
            Success = true
        };
    }
}
