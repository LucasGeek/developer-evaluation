using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

public class CartRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly CartRepository _repository;

    public CartRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new CartRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_WithValidCart_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);

        // Act
        var result = await _repository.CreateAsync(cart);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(userId, result.UserId);

        var createdCart = await _context.Carts.FindAsync(result.Id);
        Assert.NotNull(createdCart);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCart_ShouldReturnCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);

        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(cart.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cart.Id, result.Id);
        Assert.Equal(cart.UserId, result.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCart_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUser_ShouldReturnCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);

        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByUserIdAsync(nonExistentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCart_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = new Cart(userId);

        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        // Add an item to the cart
        cart.AddProduct(productId, 2);

        // Act
        var result = await _repository.UpdateAsync(cart);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cart.Id, result.Id);
        Assert.Single(result.Products);

        var updatedCart = await _context.Carts.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == cart.Id);
        Assert.NotNull(updatedCart);
        Assert.Single(updatedCart.Products);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCart_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);

        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(cart.Id);

        // Assert
        var deletedCart = await _context.Carts.FindAsync(cart.Id);
        Assert.Null(deletedCart);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var carts = new List<Cart>();
        for (int i = 0; i < 5; i++)
        {
            carts.Add(new Cart(userId));
        }

        await _context.Carts.AddRangeAsync(carts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(1, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.TotalPages);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}