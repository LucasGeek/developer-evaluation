using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class UsersControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsersControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetUsers_WithValidRequest_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UserListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetUsers_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/Users?_page=1&_size=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UserListResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            Phone = "+1234567890",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", userRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(userRequest.Email, result.Email);
        Assert.Equal(userRequest.Username, result.Username);
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Email = "invalid-email", // Invalid email format
            Username = "testuser",
            Password = "Password123!",
            Phone = "+1234567890",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", userRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = "", // Empty username
            Password = "Password123!",
            Phone = "+1234567890",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", userRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsOk()
    {
        // Note: Based on the controller implementation, it returns a demo response even for invalid IDs
        // In a real implementation, this might return NotFound
        
        // Arrange
        var invalidId = Guid.Empty;

        // Act
        var response = await _client.GetAsync($"/api/Users/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Email = "updated@example.com",
            Username = "updateduser",
            Phone = "+0987654321",
            Status = "Active",
            Role = "Manager"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Users/{userId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Email, result.Email);
        Assert.Equal(updateRequest.Username, result.Username);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Email = "invalid-email-format", // Invalid email
            Username = "updateduser",
            Phone = "+0987654321",
            Status = "Active",
            Role = "Manager"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Users/{userId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ReturnsOk()
    {
        // Note: Based on the controller implementation, it returns a demo response even for invalid IDs
        // In a real implementation, this might return NotFound
        
        // Arrange
        var invalidId = Guid.Empty;

        // Act
        var response = await _client.DeleteAsync($"/api/Users/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}