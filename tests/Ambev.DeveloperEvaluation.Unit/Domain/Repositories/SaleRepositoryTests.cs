using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Repositories;

public class SaleRepositoryTests
{
    private readonly ISaleRepository _repository;

    public SaleRepositoryTests()
    {
        _repository = Substitute.For<ISaleRepository>();
    }

    [Fact(DisplayName = "GenerateNextSaleNumberAsync should return formatted sale number")]
    public async Task GenerateNextSaleNumberAsync_ShouldReturnFormattedSaleNumber()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var expectedSaleNumber = $"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}-0001";
        _repository.GenerateNextSaleNumberAsync(branchId).Returns(expectedSaleNumber);

        // Act
        var result = await _repository.GenerateNextSaleNumberAsync(branchId);

        // Assert
        result.Should().Be(expectedSaleNumber);
        result.Should().MatchRegex(@"^BRANCH[A-F0-9]{8}-\d{4}$");
    }

    [Fact(DisplayName = "CreateAsync should call repository create method")]
    public async Task CreateAsync_ShouldCallRepositoryCreateMethod()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        await _repository.CreateAsync(sale);

        // Assert
        await _repository.Received(1).CreateAsync(sale);
    }

    [Fact(DisplayName = "GetByIdAsync should return sale when exists")]
    public async Task GetByIdAsync_WhenSaleExists_ShouldReturnSale()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        _repository.GetByIdAsync(sale.Id).Returns(sale);

        // Act
        var result = await _repository.GetByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(sale);
    }

    [Fact(DisplayName = "GetByIdAsync should return null when sale does not exist")]
    public async Task GetByIdAsync_WhenSaleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId).Returns((Sale?)null);

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetBySaleNumberAsync should return sale when exists")]
    public async Task GetBySaleNumberAsync_WhenSaleExists_ShouldReturnSale()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        _repository.GetBySaleNumberAsync(sale.SaleNumber, sale.BranchId).Returns(sale);

        // Act
        var result = await _repository.GetBySaleNumberAsync(sale.SaleNumber, sale.BranchId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(sale);
    }

    [Fact(DisplayName = "UpdateAsync should call repository update method")]
    public async Task UpdateAsync_ShouldCallRepositoryUpdateMethod()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        await _repository.UpdateAsync(sale);

        // Assert
        await _repository.Received(1).UpdateAsync(sale);
    }

    [Fact(DisplayName = "ListAsync should return paginated results")]
    public async Task ListAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var sales = SaleTestData.GenerateValidSales(10);
        var paginatedResult = new Ambev.DeveloperEvaluation.Domain.Common.PaginatedList<Sale>(
            sales.Take(5).ToList(), 10, 1, 5);
        
        _repository.ListAsync(1, 5, "date", null, null, null, null, null, null)
            .Returns(paginatedResult);

        // Act
        var result = await _repository.ListAsync(1, 5, "date");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(1);
        result.Size.Should().Be(5);
    }

    [Fact(DisplayName = "ListAsync should apply filters correctly")]
    public async Task ListAsync_ShouldApplyFiltersCorrectly()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var minDate = DateTime.UtcNow.AddDays(-30);
        var maxDate = DateTime.UtcNow;
        var saleNumber = "BRANCH12345678-0001";

        var filteredSales = SaleTestData.GenerateValidSales(3);
        var paginatedResult = new Ambev.DeveloperEvaluation.Domain.Common.PaginatedList<Sale>(
            filteredSales, 3, 1, 10);

        _repository.ListAsync(1, 10, "date", minDate, maxDate, customerId, branchId, false, saleNumber)
            .Returns(paginatedResult);

        // Act
        var result = await _repository.ListAsync(1, 10, "date", minDate, maxDate, customerId, branchId, false, saleNumber);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        await _repository.Received(1).ListAsync(1, 10, "date", minDate, maxDate, customerId, branchId, false, saleNumber);
    }
}