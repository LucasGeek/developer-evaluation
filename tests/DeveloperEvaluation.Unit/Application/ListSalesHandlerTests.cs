using DeveloperEvaluation.Application.Sales.ListSales;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListSalesHandler> _logger;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<ListSalesHandler>>();
        _handler = new ListSalesHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Handle should return paginated sales list")]
    public async Task Handle_WhenSalesExist_ShouldReturnPaginatedList()
    {
        // Arrange
        var page = 1;
        var size = 10;
        var query = new ListSalesQuery(page, size);
        
        var sales = new List<Sale>
        {
            SaleTestData.GenerateSaleWithItems(2),
            SaleTestData.GenerateSaleWithItems(3)
        };
        
        var totalCount = 15;
        _saleRepository.ListAsync(page, size, null, null, null, null, null, null, null)
            .Returns((sales, totalCount));

        var expectedItems = new List<ListSaleItemResult>
        {
            new() { Id = Guid.NewGuid(), SaleNumber = "SALE-001" },
            new() { Id = Guid.NewGuid(), SaleNumber = "SALE-002" }
        };
        _mapper.Map<List<ListSaleItemResult>>(sales).Returns(expectedItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Sales.Should().HaveCount(2);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(page);
        result.Size.Should().Be(size);
        result.TotalPages.Should().Be(2); // Math.Ceiling(15/10) = 2
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle should apply filters correctly")]
    public async Task Handle_WithFilters_ShouldApplyFiltersToRepository()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var minDate = DateTime.UtcNow.AddDays(-30);
        var maxDate = DateTime.UtcNow;
        var cancelled = false;
        var saleNumber = "SALE-123";
        var order = "date_desc";

        var query = new ListSalesQuery(1, 10, order)
        {
            CustomerId = customerId,
            BranchId = branchId,
            MinDate = minDate,
            MaxDate = maxDate,
            Cancelled = cancelled,
            SaleNumber = saleNumber
        };

        var sales = new List<Sale>();
        _saleRepository.ListAsync(1, 10, order, minDate, maxDate, customerId, branchId, cancelled, saleNumber)
            .Returns((sales, 0));
        _mapper.Map<List<ListSaleItemResult>>(sales).Returns(new List<ListSaleItemResult>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).ListAsync(1, 10, order, minDate, maxDate, customerId, branchId, cancelled, saleNumber);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle should calculate pagination correctly for last page")]
    public async Task Handle_OnLastPage_ShouldCalculatePaginationCorrectly()
    {
        // Arrange
        var page = 3;
        var size = 10;
        var totalCount = 25; // 3 pages total
        var query = new ListSalesQuery(page, size);
        
        var sales = new List<Sale> { SaleTestData.GenerateSaleWithItems(1) }; // Last page has 5 items
        _saleRepository.ListAsync(page, size, null, null, null, null, null, null, null)
            .Returns((sales, totalCount));
        _mapper.Map<List<ListSaleItemResult>>(sales).Returns(new List<ListSaleItemResult> { new() });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(page);
        result.TotalPages.Should().Be(3); // Math.Ceiling(25/10) = 3
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle should return empty list when no sales found")]
    public async Task Handle_WhenNoSalesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new ListSalesQuery(1, 10);
        
        _saleRepository.ListAsync(1, 10, null, null, null, null, null, null, null)
            .Returns((new List<Sale>(), 0));
        _mapper.Map<List<ListSaleItemResult>>(Arg.Any<List<Sale>>()).Returns(new List<ListSaleItemResult>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Sales.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Theory(DisplayName = "Handle should validate query parameters")]
    [InlineData(0, 10, 1, 10)] // Page 0 -> 1
    [InlineData(-5, 10, 1, 10)] // Negative page -> 1
    [InlineData(1, 0, 1, 10)] // Size 0 -> 10
    [InlineData(1, -10, 1, 10)] // Negative size -> 10
    [InlineData(1, 150, 1, 100)] // Size > 100 -> 100
    public async Task Handle_WithInvalidQueryParams_ShouldUseValidDefaults(
        int inputPage, int inputSize, int expectedPage, int expectedSize)
    {
        // Arrange
        var query = new ListSalesQuery(inputPage, inputSize);
        
        _saleRepository.ListAsync(expectedPage, expectedSize, null, null, null, null, null, null, null)
            .Returns((new List<Sale>(), 0));
        _mapper.Map<List<ListSaleItemResult>>(Arg.Any<List<Sale>>()).Returns(new List<ListSaleItemResult>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).ListAsync(expectedPage, expectedSize, null, null, null, null, null, null, null);
        result.Page.Should().Be(expectedPage);
        result.Size.Should().Be(expectedSize);
    }

    [Fact(DisplayName = "Handle should call repository and mapper correctly")]
    public async Task Handle_ShouldCallRepositoryAndMapperCorrectly()
    {
        // Arrange
        var query = new ListSalesQuery(2, 5, "date_desc");
        var sales = new List<Sale> { SaleTestData.GenerateSaleWithItems(1) };
        _saleRepository.ListAsync(2, 5, "date_desc", null, null, null, null, null, null)
            .Returns((sales, 1));
        _mapper.Map<List<ListSaleItemResult>>(sales).Returns(new List<ListSaleItemResult> { new() });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).ListAsync(2, 5, "date_desc", null, null, null, null, null, null);
        _mapper.Received(1).Map<List<ListSaleItemResult>>(sales);
        result.Should().NotBeNull();
        result.Sales.Should().HaveCount(1);
    }
}