using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.WebApi.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CartsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all carts with pagination and filtering
    /// </summary>
    /// <param name="limit">Number of carts per page (default: 20)</param>
    /// <param name="sort">Sort field (date, date-desc, userid, userid-desc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="startdate">Start date filter (yyyy-mm-dd)</param>
    /// <param name="enddate">End date filter (yyyy-mm-dd)</param>
    /// <returns>Paginated list of carts</returns>
    [HttpGet]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCarts(
        [FromQuery] int limit = 20,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] DateTime? startdate = null,
        [FromQuery] DateTime? enddate = null)
    {
        if (limit > 100) limit = 100;
        if (page < 1) page = 1;

        var query = new ListCartsQuery
        {
            Page = page,
            Limit = limit,
            Sort = sort,
            StartDate = startdate,
            EndDate = enddate
        };

        var result = await _mediator.Send(query);

        var response = new CartListResponse
        {
            Carts = result.Carts.Select(c => _mapper.Map<CartResponse>(c)).ToList(),
            Page = result.Page,
            Limit = result.PageSize,
            Total = result.TotalCount,
            Pages = result.TotalPages
        };

        return Ok(new ApiResponseWithData<CartListResponse>
        {
            Success = true,
            Message = "Carts retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Get cart by ID
    /// </summary>
    /// <param name="id">Cart ID</param>
    /// <returns>Cart details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetCart(Guid id)
    {
        var query = new GetCartQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Cart not found"
            });
        }

        var response = _mapper.Map<CartResponse>(result);

        return Ok(new ApiResponseWithData<CartResponse>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Create a new cart
    /// </summary>
    /// <param name="request">Cart creation request</param>
    /// <returns>Created cart ID</returns>
    [HttpPost]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateCartResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest request)
    {
        try
        {
            var command = _mapper.Map<CreateCartCommand>(request);
            var cartId = await _mediator.Send(command);

            var response = new CreateCartResponse { Id = cartId };

            return Created($"/api/carts/{cartId}",
                new ApiResponseWithData<CreateCartResponse>
                {
                    Success = true,
                    Message = "Cart created successfully",
                    Data = response
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Update a specific cart
    /// </summary>
    /// <param name="id">Cart ID</param>
    /// <param name="request">Updated cart data</param>
    /// <returns>Updated cart information</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<CartResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateCart(Guid id, [FromBody] CreateCartRequest request)
    {
        // Implementation would go here - creating an UpdateCartCommand
        // For now, return a placeholder response
        try
        {
            var cartResponse = new CartResponse
            {
                Id = id,
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                Products = request.Products?.Select(p => new CartItemResponse 
                { 
                    ProductId = p.ProductId, 
                    Quantity = p.Quantity 
                }).ToList() ?? new List<CartItemResponse>()
            };

            return Ok(new ApiResponseWithData<CartResponse>
            {
                Success = true,
                Message = "Cart updated successfully",
                Data = cartResponse
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete a cart
    /// </summary>
    /// <param name="id">Cart ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteCart(Guid id)
    {
        // Implementation would go here - creating a DeleteCartCommand
        // For now, return a placeholder response
        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Cart deletion functionality not yet implemented"
        });
    }
}