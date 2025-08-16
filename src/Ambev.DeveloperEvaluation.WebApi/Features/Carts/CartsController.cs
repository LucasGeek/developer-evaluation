using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.WebApi.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

[ApiController]
[Route("api/[controller]")]
[Tags("Carts")]
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
    /// <param name="_size">Number of carts per page (default: 10)</param>
    /// <param name="_order">Ordering of results (e.g., "id desc, userId asc")</param>
    /// <param name="_page">Page number (default: 1)</param>
    /// <returns>Paginated list of carts</returns>
    [HttpGet]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(CartListResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetCarts(
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] int _page = 1)
    {
        if (_size > 100) _size = 100;
        if (_page < 1) _page = 1;

        var query = new ListCartsQuery
        {
            Page = _page,
            Limit = _size,
            Sort = _order
        };

        var result = await _mediator.Send(query);

        var response = new CartListResponse
        {
            Data = result.Carts.Select(c => _mapper.Map<CartResponse>(c)).ToList(),
            CurrentPage = result.Page,
            TotalItems = result.TotalCount,
            TotalPages = result.TotalPages
        };

        return Ok(response);
    }

    /// <summary>
    /// Get cart by ID
    /// </summary>
    /// <param name="id">Cart ID</param>
    /// <returns>Cart details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
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
        return Ok(response);
    }

    /// <summary>
    /// Create a new cart
    /// </summary>
    /// <param name="request">Cart creation request</param>
    /// <returns>Created cart ID</returns>
    [HttpPost]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(CartResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartRequest request)
    {
        try
        {
            var command = _mapper.Map<CreateCartCommand>(request);
            var cartId = await _mediator.Send(command);

            var cartResponse = new CartResponse 
            { 
                Id = cartId,
                UserId = request.UserId,
                Date = request.Date,
                Products = request.Products?.Select(p => new CartItemResponse 
                { 
                    ProductId = p.ProductId, 
                    Quantity = p.Quantity 
                }).ToList() ?? new List<CartItemResponse>()
            };

            return Created($"/api/carts/{cartResponse.Id}", cartResponse);
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
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateCart(Guid id, [FromBody] CreateCartRequest request)
    {
        try
        {
            var command = new UpdateCartCommand
            {
                Id = id,
                UserId = request.UserId,
                Products = request.Products?.Select(p => new Ambev.DeveloperEvaluation.Application.Carts.UpdateCart.CartItemRequest 
                { 
                    ProductId = p.ProductId, 
                    Quantity = p.Quantity 
                }).ToList() ?? new List<Ambev.DeveloperEvaluation.Application.Carts.UpdateCart.CartItemRequest>()
            };

            var success = await _mediator.Send(command);
            
            if (!success)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cart not found"
                });
            }

            // Get updated cart to return in response
            var getCartQuery = new GetCartQuery(id);
            var updatedCart = await _mediator.Send(getCartQuery);
            
            if (updatedCart == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cart not found after update"
                });
            }

            var cartResponse = _mapper.Map<CartResponse>(updatedCart);
            return Ok(cartResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
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
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer,Manager,Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteCart(Guid id)
    {
        try
        {
            // For this implementation, we need to get the user ID from the current user context
            // In a real application, this would come from the JWT token or current user context
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Unable to identify user"
                });
            }

            var command = new DeleteCartCommand(id, userId);
            var success = await _mediator.Send(command);
            
            if (!success)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cart not found"
                });
            }

            return Ok(new { message = "Cart deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message
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
}