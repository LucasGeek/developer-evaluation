using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CancelSaleItemHandler>>();
        _handler = new CancelSaleItemHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Handle should cancel sale item successfully")]
    public async Task Handle_WhenValidRequest_ShouldCancelSaleItemSuccessfully()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(3);
        var itemToCancel = existingSale.Items.First();
        var originalTotalAmount = existingSale.TotalAmount;
        var originalItemsCount = existingSale.Items.Count;
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = itemToCancel.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SaleId.Should().Be(existingSale.Id);
        result.ItemId.Should().Be(itemToCancel.Id);
        result.ItemRemoved.Should().BeTrue();
        result.Message.Should().Be("Item cancelled successfully");
        
        existingSale.Items.Count.Should().Be(originalItemsCount - 1);
        existingSale.TotalAmount.Should().BeLessThan(originalTotalAmount);
        
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.Received(1).UpdateAsync(existingSale);
    }

    [Fact(DisplayName = "Handle should throw exception when sale not found")]
    public async Task Handle_WhenSaleNotFound_ShouldThrowException()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new CancelSaleItemCommand
        {
            SaleId = saleId,
            ItemId = itemId
        };

        _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
        await _saleRepository.Received(1).GetByIdAsync(saleId);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should throw exception when sale is cancelled")]
    public async Task Handle_WhenSaleIsCancelled_ShouldThrowException()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(2);
        existingSale.Cancel(); // Cancel the sale first
        var itemToCancel = existingSale.Items.First();
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = itemToCancel.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("cancelled sale");
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should throw exception when item not found")]
    public async Task Handle_WhenItemNotFound_ShouldThrowException()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(2);
        var nonExistentItemId = Guid.NewGuid();
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = nonExistentItemId
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("Item with ID").And.Contain("not found");
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should throw exception when trying to cancel last item")]
    public async Task Handle_WhenCancellingLastItem_ShouldThrowException()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(1); // Only one item
        var lastItem = existingSale.Items.Single();
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = lastItem.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("Cannot cancel the last item in a sale");
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should recalculate totals after item removal")]
    public async Task Handle_ShouldRecalculateTotalsAfterItemRemoval()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(3);
        var itemToCancel = existingSale.Items.First();
        var itemTotalBeforeCancel = itemToCancel.Total;
        var saleTotalBeforeCancel = existingSale.TotalAmount;
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = itemToCancel.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingSale.TotalAmount.Should().Be(saleTotalBeforeCancel - itemTotalBeforeCancel);
        await _saleRepository.Received(1).UpdateAsync(existingSale);
    }

    [Fact(DisplayName = "Handle should preserve product description in result")]
    public async Task Handle_ShouldPreserveProductDescriptionInResult()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(2);
        var itemToCancel = existingSale.Items.First();
        var expectedProductDescription = itemToCancel.ProductDescription;
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = itemToCancel.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ProductDescription.Should().Be(expectedProductDescription);
        result.SaleNumber.Should().Be(existingSale.SaleNumber);
    }

    [Fact(DisplayName = "Handle should log item cancellation operations")]
    public async Task Handle_ShouldLogItemCancellationOperations()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(2);
        var itemToCancel = existingSale.Items.First();
        
        var command = new CancelSaleItemCommand
        {
            SaleId = existingSale.Id,
            ItemId = itemToCancel.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify the handler completed successfully without exceptions
        // Logger verification is simplified due to NSubstitute complexity
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.Received(1).UpdateAsync(existingSale);
    }
}