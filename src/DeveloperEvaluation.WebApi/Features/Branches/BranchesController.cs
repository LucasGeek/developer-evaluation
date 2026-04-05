using DeveloperEvaluation.Application.Branches.CreateBranch;
using DeveloperEvaluation.Application.Branches.DeleteBranch;
using DeveloperEvaluation.Application.Branches.GetBranch;
using DeveloperEvaluation.Application.Branches.ListBranches;
using DeveloperEvaluation.Application.Branches.UpdateBranch;
using DeveloperEvaluation.WebApi.Common;
using DeveloperEvaluation.WebApi.Features.Branches.CreateBranch;
using DeveloperEvaluation.WebApi.Features.Branches.GetBranch;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperEvaluation.WebApi.Features.Branches;

[ApiController]
[Route("api/[controller]")]
[Tags("Branches")]
[Authorize]
public class BranchesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public BranchesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new branch
    /// </summary>
    /// <param name="request">The branch creation request</param>
    /// <returns>The created branch ID</returns>
    /// <response code="201">Branch created successfully</response>
    /// <response code="400">Invalid branch data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateBranchResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
    {
        var command = _mapper.Map<CreateBranchCommand>(request);
        var branchId = await _mediator.Send(command);
        
        var response = new CreateBranchResponse { Id = branchId };
        
        return Created($"/api/branches/{branchId}", 
            new ApiResponseWithData<CreateBranchResponse>
            {
                Success = true,
                Message = "Branch created successfully",
                Data = response
            });
    }

    /// <summary>
    /// Retrieves a specific branch by ID
    /// </summary>
    /// <param name="id">The branch ID</param>
    /// <returns>The branch details</returns>
    /// <response code="200">Branch retrieved successfully</response>
    /// <response code="404">Branch not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetBranchResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetBranchQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound("Branch not found");
        }
        
        var response = _mapper.Map<GetBranchResponse>(result);
        return Ok(new ApiResponseWithData<GetBranchResponse>
        {
            Success = true,
            Message = "Branch retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Retrieves all branches with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10, max: 100)</param>
    /// <param name="activeOnly">Return only active branches (default: false)</param>
    /// <returns>Paginated list of branches</returns>
    /// <response code="200">Branches retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetBranchResponse>), 200)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] bool activeOnly = false)
    {
        var query = new ListBranchesQuery(page, size, activeOnly);
        var result = await _mediator.Send(query);
        
        var branches = _mapper.Map<List<GetBranchResponse>>(result.Branches);
        
        return Ok(new PaginatedResponse<GetBranchResponse>
        {
            Success = true,
            Message = "Branches retrieved successfully",
            Data = new PaginatedList<GetBranchResponse>(branches, result.TotalCount, result.CurrentPage, size),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

    /// <summary>
    /// Updates an existing branch
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetBranchResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateBranchRequest request)
    {
        var command = new UpdateBranchCommand(
            id, request.Name, request.Description, request.Address,
            request.City, request.State, request.PostalCode, request.Phone, true);

        var success = await _mediator.Send(command);
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Branch not found" });

        var branch = await _mediator.Send(new GetBranchQuery(id));
        var response = _mapper.Map<GetBranchResponse>(branch);
        return Ok(new ApiResponseWithData<GetBranchResponse> { Success = true, Message = "Branch updated successfully", Data = response });
    }

    /// <summary>
    /// Deletes a branch
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteBranchCommand(id));
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Branch not found" });

        return Ok(new ApiResponse { Success = true, Message = "Branch deleted successfully" });
    }
}