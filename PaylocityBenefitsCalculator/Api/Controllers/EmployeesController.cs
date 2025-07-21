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

    [SwaggerOperation(Summary = "Get all employees")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetEmployeeDto>>>> GetAll()
    {
        var employeeDataResponse = await _employeeService.GetAllAsync();

        var result = new ApiResponse<List<GetEmployeeDto>>
        {
            Data = employeeDataResponse.EmployeeData,
            Success = true
        };

        return result;
    }

    [SwaggerOperation(Summary = "Get employee by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetEmployeeDto>>> Get(int id)
    {
        var employeeDataResponse = await _employeeService.GetEmployeeAsync(id);

        if (employeeDataResponse.Status == Status.NotFound)
            return NotFound();

        if (employeeDataResponse.Status == Status.InvalidData)
            return StatusCode(500, employeeDataResponse.Message);

        return new ApiResponse<GetEmployeeDto>
        {
            Data = employeeDataResponse.EmployeeData,
            Success = true
        };
    }
}
