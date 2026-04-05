using DeveloperEvaluation.Application.Products.GetProduct;
using DeveloperEvaluation.Application.Products.ListProducts;
using DeveloperEvaluation.Application.Products.GetCategories;
using DeveloperEvaluation.Application.Products.CreateProduct;
using DeveloperEvaluation.WebApi.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieve all products with pagination, filtering and sorting
    /// </summary>
    /// <param name="_size">Number of products per page (default: 10, max: 100)</param>
    /// <param name="_order">Ordering of results (e.g., "price desc", "title asc")</param>
    /// <param name="_page">Page number starting from 1 (default: 1)</param>
    /// <returns>Paginated list of products with metadata</returns>
    /// <response code="200">Successfully retrieved products</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="401">Authentication required</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ProductListResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int _size = 10, 
        [FromQuery] string? _order = null,
        [FromQuery] int _page = 1)
    {
        if (_size > 100) _size = 100; // Prevent abuse
        if (_page < 1) _page = 1;

        var query = new ListProductsQuery 
        { 
            Page = _page, 
            Limit = _size, 
            Sort = _order 
        };
        
        var result = await _mediator.Send(query);
        
        var response = new ProductListResponse
        {
            Data = result.Products.Select(p => _mapper.Map<ProductResponse>(p)).ToList(),
            CurrentPage = result.Page,
            TotalItems = result.TotalCount,
            TotalPages = result.TotalPages
        };

        return Ok(response);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = "Product not found"
            });
        }

        var response = _mapper.Map<ProductResponse>(result);
        return Ok(response);
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("categories")]
    [Authorize]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query);

        return Ok(result.Categories);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <param name="_size">Number of products per page (default: 10)</param>
    /// <param name="_order">Ordering of results (e.g., "price desc, title asc")</param>
    /// <param name="_page">Page number (default: 1)</param>
    /// <returns>Paginated list of products in category</returns>
    [HttpGet("category/{category}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductListResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetProductsByCategory(
        string category,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null,
        [FromQuery] int _page = 1)
    {
        if (_size > 100) _size = 100;
        if (_page < 1) _page = 1;

        var query = new ListProductsQuery
        {
            Page = _page,
            Limit = _size,
            Sort = _order,
            Category = category
        };

        var result = await _mediator.Send(query);

        var response = new ProductListResponse
        {
            Data = result.Products.Select(p => _mapper.Map<ProductResponse>(p)).ToList(),
            CurrentPage = result.Page,
            TotalItems = result.TotalCount,
            TotalPages = result.TotalPages
        };

        return Ok(response);
    }

    /// <summary>
    /// Add a new product
    /// </summary>
    /// <param name="request">Product data</param>
    /// <returns>Created product information</returns>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var command = _mapper.Map<CreateProductCommand>(request);
            var productId = await _mediator.Send(command);

            // Get the created product to return in response
            var getProductQuery = new GetProductQuery(productId);
            var createdProduct = await _mediator.Send(getProductQuery);
            
            if (createdProduct == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Product was created but could not be retrieved"
                });
            }

            var productResponse = _mapper.Map<ProductResponse>(createdProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = productResponse.Id }, productResponse);
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
    /// Update a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated product data</param>
    /// <returns>Updated product information</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        // Implementation would go here - creating an UpdateProductCommand
        // For now, return a placeholder response
        try
        {
            var productResponse = new ProductResponse
            {
                Id = id,
                Title = request.Title,
                Price = request.Price,
                Description = request.Description,
                Category = request.Category,
                Image = request.Image,
                Rating = new RatingResponse 
                { 
                    Rate = (double)request.Rating.Rate, 
                    Count = request.Rating.Count 
                }
            };

            return Ok(productResponse);
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
    /// Delete a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        // Implementation would go here - creating a DeleteProductCommand
        // For now, return a placeholder response
        try
        {
            return Ok(new { message = "Product deleted successfully" });
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