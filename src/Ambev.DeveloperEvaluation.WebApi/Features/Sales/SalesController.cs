using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    /// <param name="request">The sale creation request</param>
    /// <returns>The created sale ID</returns>
    /// <response code="201">Sale created successfully</response>
    /// <response code="400">Invalid sale data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        var command = _mapper.Map<CreateSaleCommand>(request);
        var saleId = await _mediator.Send(command);
        
        var response = new CreateSaleResponse { Id = saleId };
        
        return Created($"/api/sales/{saleId}", 
            new ApiResponseWithData<CreateSaleResponse>
            {
                Success = true,
                Message = "Sale created successfully",
                Data = response
            });
    }

    /// <summary>
    /// Retrieves a sale by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <returns>The sale details if found</returns>
    /// <response code="200">Sale found and returned successfully</response>
    /// <response code="404">Sale not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSaleQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Sale not found"
            });
        }

        var response = _mapper.Map<GetSaleResponse>(result);
        
        return Ok(new ApiResponseWithData<GetSaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Lists sales with pagination and filters
    /// </summary>
    /// <param name="request">The list sales request with pagination and filters</param>
    /// <returns>A paginated list of sales</returns>
    /// <response code="200">Sales list retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<ListSalesResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> List([FromQuery] ListSalesRequest request)
    {
        var query = _mapper.Map<ListSalesQuery>(request);
        var result = await _mediator.Send(query);
        
        var response = _mapper.Map<ListSalesResponse>(result);
        
        return Ok(new ApiResponseWithData<ListSalesResponse>
        {
            Success = true,
            Message = "Sales list retrieved successfully",
            Data = response
        });
    }
}