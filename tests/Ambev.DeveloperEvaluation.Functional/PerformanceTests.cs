using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts;
using Ambev.DeveloperEvaluation.WebApi.Features.Products;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class PerformanceTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PerformanceTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetProducts_ResponseTime_ShouldBeFast()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/Products");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Response time was {stopwatch.ElapsedMilliseconds}ms, expected less than 5000ms");
    }

    [Fact]
    public async Task ConcurrentRequests_GetProducts_ShouldHandleLoad()
    {
        // Arrange
        const int numberOfRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < numberOfRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/Products"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => 
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }

    [Fact]
    public async Task BulkCartCreation_ShouldHandleMultipleRequests()
    {
        // Arrange
        const int numberOfCarts = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < numberOfCarts; i++)
        {
            var cartRequest = new CreateCartRequest
            {
                UserId = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Products = new List<CartItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            tasks.Add(_client.PostAsJsonAsync("/api/Carts", cartRequest));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => 
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        });
    }

    [Fact]
    public async Task LargePageSize_GetProducts_ShouldHandleEfficiently()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/Products?_size=100");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Large page response time was {stopwatch.ElapsedMilliseconds}ms, expected less than 10000ms");
    }

    [Fact]
    public async Task SequentialCrudOperations_ShouldMaintainPerformance()
    {
        // Arrange
        var totalStopwatch = Stopwatch.StartNew();
        var operationTimes = new Dictionary<string, long>();

        // Create User
        var createUserStopwatch = Stopwatch.StartNew();
        var createUserRequest = new CreateUserRequest
        {
            Email = "perf@test.com",
            Username = "perfuser",
            Password = "Password123!",
            Phone = "+1234567890",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };

        var userResponse = await _client.PostAsJsonAsync("/api/Users", createUserRequest);
        createUserStopwatch.Stop();
        operationTimes["CreateUser"] = createUserStopwatch.ElapsedMilliseconds;

        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        var createdUser = JsonSerializer.Deserialize<GetUserResponse>(
            await userResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Create Cart
        var createCartStopwatch = Stopwatch.StartNew();
        var createCartRequest = new CreateCartRequest
        {
            UserId = createdUser!.Id,
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };

        var cartResponse = await _client.PostAsJsonAsync("/api/Carts", createCartRequest);
        createCartStopwatch.Stop();
        operationTimes["CreateCart"] = createCartStopwatch.ElapsedMilliseconds;

        Assert.Equal(HttpStatusCode.Created, cartResponse.StatusCode);
        var createdCart = JsonSerializer.Deserialize<CartResponse>(
            await cartResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Update Cart
        var updateCartStopwatch = Stopwatch.StartNew();
        var updateCartRequest = new CreateCartRequest
        {
            UserId = createdUser.Id,
            Date = DateTime.UtcNow,
            Products = new List<CartItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 3 }
            }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/Carts/{createdCart!.Id}", updateCartRequest);
        updateCartStopwatch.Stop();
        operationTimes["UpdateCart"] = updateCartStopwatch.ElapsedMilliseconds;

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Delete Cart
        var deleteCartStopwatch = Stopwatch.StartNew();
        var deleteResponse = await _client.DeleteAsync($"/api/Carts/{createdCart.Id}");
        deleteCartStopwatch.Stop();
        operationTimes["DeleteCart"] = deleteCartStopwatch.ElapsedMilliseconds;

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        totalStopwatch.Stop();

        // Assert performance metrics
        Assert.True(totalStopwatch.ElapsedMilliseconds < 15000, 
            $"Total CRUD operations took {totalStopwatch.ElapsedMilliseconds}ms, expected less than 15000ms");

        foreach (var operation in operationTimes)
        {
            Assert.True(operation.Value < 5000, 
                $"{operation.Key} took {operation.Value}ms, expected less than 5000ms");
        }
    }

    [Fact]
    public async Task PaginationPerformance_DifferentPageSizes_ShouldScaleWell()
    {
        // Test different page sizes and measure performance
        var pageSizes = new[] { 5, 10, 25, 50, 100 };
        var results = new Dictionary<int, long>();

        foreach (var pageSize in pageSizes)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var response = await _client.GetAsync($"/api/Products?_size={pageSize}");
            
            stopwatch.Stop();
            results[pageSize] = stopwatch.ElapsedMilliseconds;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Performance should not degrade dramatically with larger page sizes
        // (though this depends on the actual implementation and data size)
        foreach (var result in results)
        {
            Assert.True(result.Value < 10000, 
                $"Page size {result.Key} took {result.Value}ms, expected less than 10000ms");
        }
    }

    [Fact]
    public async Task ApiHealthCheck_ShouldRespondQuickly()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/Health");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Health check took {stopwatch.ElapsedMilliseconds}ms, expected less than 1000ms");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ConcurrentUserCreation_ShouldHandleLoad(int concurrentUsers)
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < concurrentUsers; i++)
        {
            var userRequest = new CreateUserRequest
            {
                Email = $"concurrent{i}@test.com",
                Username = $"concurrent{i}",
                Password = "Password123!",
                Phone = $"+123456789{i}",
                Status = UserStatus.Active,
                Role = UserRole.Customer
            };

            tasks.Add(_client.PostAsJsonAsync("/api/Users", userRequest));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.All(responses, response => 
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        });

        // Performance should be reasonable even with concurrent requests
        var avgTimePerRequest = stopwatch.ElapsedMilliseconds / concurrentUsers;
        Assert.True(avgTimePerRequest < 5000, 
            $"Average time per concurrent user creation was {avgTimePerRequest}ms, expected less than 5000ms");
    }
}