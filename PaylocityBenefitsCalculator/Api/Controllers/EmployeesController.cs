using Api.Dtos.Employee;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

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

    [SwaggerOperation(Summary = "Get employee by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetEmployeeDto>>> Get(int id)
    {
        var employeeDataResponse = await _employeeService.GetEmployeeAsync(id);

        if (employeeDataResponse.StatusCode != HttpStatusCode.OK) 
        {
            return StatusCode((int)employeeDataResponse.StatusCode, employeeDataResponse.Message);
        }

        return new ApiResponse<GetEmployeeDto>
        {
            Data = employeeDataResponse.EmployeeData,
            Success = true
        };
    }

    [SwaggerOperation(Summary = "Get all employees")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetEmployeeDto>>>> GetAll()
    {
        var employeeDataResponse = await _employeeService.GetAllAsync();

        if (employeeDataResponse.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int)employeeDataResponse.StatusCode, employeeDataResponse.Message);
        }

        var result = new ApiResponse<List<GetEmployeeDto>>
        {
            Data = employeeDataResponse.EmployeeData,
            Success = true
        };

        return result;
    }
}
