using DeveloperEvaluation.Application.Sales.GetSale;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.Application;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Handle should return sale when exists")]
    public async Task Handle_WhenSaleExists_ShouldReturnMappedSale()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var sale = SaleTestData.GenerateSaleWithItems(2);
        typeof(Sale).GetProperty("Id")!.SetValue(sale, saleId);
        
        var expectedResult = new GetSaleResult
        {
            Id = saleId,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            Items = new List<GetSaleItemResult>()
        };

        _saleRepository.GetByIdAsync(saleId).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        var query = new GetSaleQuery(saleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedResult);
        await _saleRepository.Received(1).GetByIdAsync(saleId);
        _mapper.Received(1).Map<GetSaleResult>(sale);
    }

    [Fact(DisplayName = "Handle should return null when sale does not exist")]
    public async Task Handle_WhenSaleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(null));

        var query = new GetSaleQuery(saleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _saleRepository.Received(1).GetByIdAsync(saleId);
        _mapper.DidNotReceive().Map<GetSaleResult>(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should call repository with correct ID")]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var query = new GetSaleQuery(saleId);

        _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetByIdAsync(saleId);
    }

    [Fact(DisplayName = "Handle should return sale with items")]
    public async Task Handle_WhenSaleHasItems_ShouldReturnSaleWithItems()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var sale = SaleTestData.GenerateSaleWithItems(3);
        typeof(Sale).GetProperty("Id")!.SetValue(sale, saleId);

        var expectedResult = new GetSaleResult
        {
            Id = saleId,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            Items = new List<GetSaleItemResult>
            {
                new() { Id = Guid.NewGuid(), ProductDescription = "Item 1" },
                new() { Id = Guid.NewGuid(), ProductDescription = "Item 2" },
                new() { Id = Guid.NewGuid(), ProductDescription = "Item 3" }
            }
        };

        _saleRepository.GetByIdAsync(saleId).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        var query = new GetSaleQuery(saleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Id.Should().Be(saleId);
    }
}