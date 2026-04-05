using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Enums;
using DeveloperEvaluation.ORM;
using DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DeveloperEvaluation.Unit.ORM;

public class UserRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_WithValidUser_ShouldCreateSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Username);

        var createdUser = await _context.Users.FindAsync(result.Id);
        Assert.NotNull(createdUser);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(user.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(nonExistentEmail);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingUser_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.Username = "updateduser";
        user.Email = "updated@example.com";

        // Act
        var result = await _repository.UpdateAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updateduser", result.Username);
        Assert.Equal("updated@example.com", result.Email);

        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.Equal("updateduser", updatedUser.Username);
        Assert.Equal("updated@example.com", updatedUser.Email);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(user.Id);

        // Assert
        Assert.True(result);

        var deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}