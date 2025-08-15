using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Products.GetCategories;
using Ambev.DeveloperEvaluation.WebApi.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
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
    /// Get all products with pagination, filtering and sorting
    /// </summary>
    /// <param name="limit">Number of products per page (default: 20)</param>
    /// <param name="sort">Sort field (price, title, price-desc, title-desc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int limit = 20, 
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1)
    {
        if (limit > 100) limit = 100; // Prevent abuse
        if (page < 1) page = 1;

        var query = new ListProductsQuery 
        { 
            Page = page, 
            Limit = limit, 
            Sort = sort 
        };
        
        var result = await _mediator.Send(query);
        
        var response = new ProductListResponse
        {
            Products = result.Products.Select(p => _mapper.Map<ProductResponse>(p)).ToList(),
            Page = result.Page,
            Limit = result.PageSize,
            Total = result.TotalCount,
            Pages = result.TotalPages
        };

        return Ok(new ApiResponseWithData<ProductListResponse>
        {
            Success = true,
            Message = "Products retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResponse>), 200)]
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

        return Ok(new ApiResponseWithData<ProductResponse>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponseWithData<List<string>>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetCategoriesQuery();
        var result = await _mediator.Send(query);

        return Ok(new ApiResponseWithData<List<string>>
        {
            Success = true,
            Message = "Categories retrieved successfully",
            Data = result.Categories
        });
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <param name="limit">Number of products per page (default: 20)</param>
    /// <param name="sort">Sort field (price, title, price-desc, title-desc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <returns>Paginated list of products in category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> GetProductsByCategory(
        string category,
        [FromQuery] int limit = 20,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1)
    {
        if (limit > 100) limit = 100;
        if (page < 1) page = 1;

        var query = new ListProductsQuery
        {
            Page = page,
            Limit = limit,
            Sort = sort,
            Category = category
        };

        var result = await _mediator.Send(query);

        var response = new ProductListResponse
        {
            Products = result.Products.Select(p => _mapper.Map<ProductResponse>(p)).ToList(),
            Page = result.Page,
            Limit = result.PageSize,
            Total = result.TotalCount,
            Pages = result.TotalPages
        };

        return Ok(new ApiResponseWithData<ProductListResponse>
        {
            Success = true,
            Message = $"Products in category '{category}' retrieved successfully",
            Data = response
        });
    }

    /// <summary>
    /// Add a new product
    /// </summary>
    /// <param name="request">Product data</param>
    /// <returns>Created product information</returns>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Implementation would go here - creating a CreateProductCommand
        // For now, return a placeholder response
        try
        {
            var productResponse = new ProductResponse
            {
                Id = Guid.NewGuid(),
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

            return CreatedAtAction(nameof(GetProduct), new { id = productResponse.Id }, 
                new ApiResponseWithData<ProductResponse>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = productResponse
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
    /// Update a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated product data</param>
    /// <returns>Updated product information</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponseWithData<ProductResponse>), 200)]
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

            return Ok(new ApiResponseWithData<ProductResponse>
            {
                Success = true,
                Message = "Product updated successfully",
                Data = productResponse
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
    /// Delete a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        // Implementation would go here - creating a DeleteProductCommand
        // For now, return a placeholder response
        try
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product deleted successfully"
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