using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
[Tags("Sales")]
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
        // Extract customer ID from JWT token
        var customerId = GetCurrentUserGuid();
        
        var command = new CreateSaleCommand(
            request.BranchId,
            customerId,
            request.Items.Select(_mapper.Map<CreateSaleItemDto>).ToList()
        );
        
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

    /// <summary>
    /// Updates an existing sale
    /// </summary>
    /// <param name="id">The unique identifier of the sale to update</param>
    /// <param name="request">The sale update request</param>
    /// <returns>The updated sale details</returns>
    /// <response code="200">Sale updated successfully</response>
    /// <response code="400">Invalid sale data or business rule violation</response>
    /// <response code="404">Sale not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleRequest request)
    {
        try
        {
            var command = new UpdateSaleCommand(id, request.Date, request.Items.Select(_mapper.Map<UpdateSaleItemDto>).ToList());
            
            var result = await _mediator.Send(command);
            var response = _mapper.Map<UpdateSaleResponse>(result);
            
            return Ok(new ApiResponseWithData<UpdateSaleResponse>
            {
                Success = true,
                Message = "Sale updated successfully",
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Cancels an existing sale
    /// </summary>
    /// <param name="id">The unique identifier of the sale to cancel</param>
    /// <returns>The cancelled sale details</returns>
    /// <response code="200">Sale cancelled successfully</response>
    /// <response code="404">Sale not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var command = new CancelSaleCommand { Id = id };
            var result = await _mediator.Send(command);
            var response = _mapper.Map<CancelSaleResponse>(result);
            
            return Ok(new ApiResponseWithData<CancelSaleResponse>
            {
                Success = true,
                Message = result.Message,
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Cancels/removes a specific item from a sale
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="itemId">The unique identifier of the item to cancel</param>
    /// <returns>The result of the item cancellation</returns>
    /// <response code="200">Item cancelled successfully</response>
    /// <response code="400">Invalid request or business rule violation</response>
    /// <response code="404">Sale or item not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleItemResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> CancelItem(Guid id, Guid itemId)
    {
        try
        {
            var command = new CancelSaleItemCommand
            {
                SaleId = id,
                ItemId = itemId
            };
            
            var result = await _mediator.Send(command);
            var response = _mapper.Map<CancelSaleItemResponse>(result);
            
            return Ok(new ApiResponseWithData<CancelSaleItemResponse>
            {
                Success = true,
                Message = result.Message,
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}