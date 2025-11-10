using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DependentsController : ControllerBase
{
    private readonly IDependentRepository _dependentRepo;
    public DependentsController(IDependentRepository dependentRepo)
    {
        // Usually a repo isn't injected directly into a controller. Typically the controller calls a service or provider class that conducts some kind of business logic (even something like authorization checks to ensure that the requestor has access) and returns the desired data to the controller.
        // In this example, we aren't conducting any business logic on dependents at this time that is relevant to these 2 retrieval methods and so I opted not to add an abstraction just for the sake of having one.
        // In the future, the service could handle the paginating of data, for example
        _dependentRepo = dependentRepo;
    }

    // @auth: Admin or Employee who claims this dependent
    [SwaggerOperation(Summary = "Get dependent by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetDependentDto>>> Get(int id)
    {
        var dependent = await _dependentRepo.GetDependentAsync(id);

        if (dependent is null)
            return NotFound();

        return new ApiResponse<GetDependentDto>
        {
            Data = dependent,
            Success = true
        };
    }

    // @auth: Admin, if we later were to GetAllByEmployeeId this could be accessed by Employee with EmployeeId
    [SwaggerOperation(Summary = "Get all dependents")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetDependentDto>>>> GetAll()
    {
        // This endpoint should be paginated with values for "Limit" and "Offset" from FromQuery string params
        // These params should have default values such that we don't return the entire result set if they are left empty
        var dependents = await _dependentRepo.GetAllDependentsAsync();

        var result = new ApiResponse<List<GetDependentDto>>
        {
            Data = dependents,
            Success = true
        };

        return result;
    }
}
