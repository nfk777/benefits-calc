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
        _dependentRepo = dependentRepo;
    }

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

    [SwaggerOperation(Summary = "Get all dependents")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetDependentDto>>>> GetAll()
    {
        var dependents = await _dependentRepo.GetAllDependentsAsync();

        var result = new ApiResponse<List<GetDependentDto>>
        {
            Data = dependents,
            Success = true
        };

        return result;
    }
}
