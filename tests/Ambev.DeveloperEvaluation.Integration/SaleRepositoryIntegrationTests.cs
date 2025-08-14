using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class SaleRepositoryIntegrationTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly SaleRepository _repository;

    public SaleRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new SaleRepository(_context);
    }

    [Fact(DisplayName = "CreateAsync should persist sale to database")]
    public async Task CreateAsync_ShouldPersistSaleToDatabase()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.SetSaleNumber("BRANCH12345678-0001");

        // Act
        await _repository.CreateAsync(sale);

        // Assert
        var persistedSale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == sale.Id);
        persistedSale.Should().NotBeNull();
        persistedSale!.SaleNumber.Should().Be(sale.SaleNumber);
        persistedSale.TotalAmount.Should().Be(sale.TotalAmount);
    }

    [Fact(DisplayName = "GetByIdAsync should return sale with items")]
    public async Task GetByIdAsync_ShouldReturnSaleWithItems()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(2);
        sale.SetSaleNumber("BRANCH12345678-0002");
        await _repository.CreateAsync(sale);

        // Act
        var result = await _repository.GetByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.SaleNumber.Should().Be(sale.SaleNumber);
    }

    [Fact(DisplayName = "GenerateNextSaleNumberAsync should increment correctly")]
    public async Task GenerateNextSaleNumberAsync_ShouldIncrementCorrectly()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var sale1 = SaleTestData.GenerateValidSale();
        sale1.SetSaleNumber($"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}-0001");
        typeof(Sale).GetProperty("BranchId")!.SetValue(sale1, branchId);
        await _repository.CreateAsync(sale1);

        // Act
        var nextSaleNumber = await _repository.GenerateNextSaleNumberAsync(branchId);

        // Assert
        nextSaleNumber.Should().EndWith("-0002");
        nextSaleNumber.Should().StartWith($"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}");
    }

    [Fact(DisplayName = "GetBySaleNumberAsync should return correct sale")]
    public async Task GetBySaleNumberAsync_ShouldReturnCorrectSale()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var saleNumber = $"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}-0001";
        var sale = SaleTestData.GenerateValidSale();
        sale.SetSaleNumber(saleNumber);
        typeof(Sale).GetProperty("BranchId")!.SetValue(sale, branchId);
        await _repository.CreateAsync(sale);

        // Act
        var result = await _repository.GetBySaleNumberAsync(saleNumber, branchId);

        // Assert
        result.Should().NotBeNull();
        result!.SaleNumber.Should().Be(saleNumber);
        result.BranchId.Should().Be(branchId);
    }

    [Fact(DisplayName = "UpdateAsync should persist changes")]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.SetSaleNumber("BRANCH12345678-0003");
        await _repository.CreateAsync(sale);

        // Act
        sale.Cancel();
        await _repository.UpdateAsync(sale);

        // Assert
        var updatedSale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == sale.Id);
        updatedSale.Should().NotBeNull();
        updatedSale!.Cancelled.Should().BeTrue();
        updatedSale.CancelledAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "ListAsync should return paginated results")]
    public async Task ListAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        for (int i = 1; i <= 10; i++)
        {
            var sale = SaleTestData.GenerateValidSale();
            sale.SetSaleNumber($"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}-{i:D4}");
            typeof(Sale).GetProperty("BranchId")!.SetValue(sale, branchId);
            await _repository.CreateAsync(sale);
        }

        // Act
        var result = await _repository.ListAsync(1, 5, "salenumber");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(1);
        result.Size.Should().Be(5);
    }

    [Fact(DisplayName = "ListAsync should filter by branch")]
    public async Task ListAsync_ShouldFilterByBranch()
    {
        // Arrange
        var branchId1 = Guid.NewGuid();
        var branchId2 = Guid.NewGuid();

        // Create sales for branch 1
        for (int i = 1; i <= 3; i++)
        {
            var sale = SaleTestData.GenerateValidSale();
            sale.SetSaleNumber($"BRANCH{branchId1.ToString("N").Substring(0, 8).ToUpper()}-{i:D4}");
            typeof(Sale).GetProperty("BranchId")!.SetValue(sale, branchId1);
            await _repository.CreateAsync(sale);
        }

        // Create sales for branch 2
        for (int i = 1; i <= 2; i++)
        {
            var sale = SaleTestData.GenerateValidSale();
            sale.SetSaleNumber($"BRANCH{branchId2.ToString("N").Substring(0, 8).ToUpper()}-{i:D4}");
            typeof(Sale).GetProperty("BranchId")!.SetValue(sale, branchId2);
            await _repository.CreateAsync(sale);
        }

        // Act
        var result = await _repository.ListAsync(1, 10, "salenumber", branchId: branchId1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => s.BranchId == branchId1);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}