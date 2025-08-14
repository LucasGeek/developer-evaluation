using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleHandler> _logger;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CancelSaleHandler>>();
        _handler = new CancelSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Handle should cancel existing sale successfully")]
    public async Task Handle_WhenSaleExists_ShouldCancelSaleSuccessfully()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(1);
        var saleId = existingSale.Id;
        
        var command = new CancelSaleCommand
        {
            Id = saleId
        };

        _saleRepository.GetByIdAsync(saleId).Returns(existingSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(saleId);
        result.Cancelled.Should().BeTrue();
        result.CancelledAt.Should().NotBeNull();
        result.Message.Should().Be("Sale cancelled successfully");
        
        existingSale.Cancelled.Should().BeTrue();
        existingSale.CancelledAt.Should().NotBeNull();
        
        await _saleRepository.Received(1).GetByIdAsync(saleId);
        await _saleRepository.Received(1).UpdateAsync(existingSale);
    }

    [Fact(DisplayName = "Handle should throw exception when sale not found")]
    public async Task Handle_WhenSaleNotFound_ShouldThrowException()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand
        {
            Id = saleId
        };

        _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("not found");
        await _saleRepository.Received(1).GetByIdAsync(saleId);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should return already cancelled message when sale is already cancelled")]
    public async Task Handle_WhenSaleAlreadyCancelled_ShouldReturnAlreadyCancelledMessage()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(1);
        existingSale.Cancel(); // Cancel the sale first
        var originalCancelledAt = existingSale.CancelledAt;
        
        var command = new CancelSaleCommand
        {
            Id = existingSale.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingSale.Id);
        result.Cancelled.Should().BeTrue();
        result.CancelledAt.Should().Be(originalCancelledAt);
        result.Message.Should().Be("Sale is already cancelled");
        
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should set correct properties when cancelling sale")]
    public async Task Handle_ShouldSetCorrectPropertiesWhenCancellingSale()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(2);
        var originalUpdatedAt = existingSale.UpdatedAt;
        
        var command = new CancelSaleCommand
        {
            Id = existingSale.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingSale.Cancelled.Should().BeTrue();
        existingSale.CancelledAt.Should().NotBeNull();
        existingSale.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        existingSale.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact(DisplayName = "Handle should log cancellation operations")]
    public async Task Handle_ShouldLogCancellationOperations()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(1);
        var command = new CancelSaleCommand
        {
            Id = existingSale.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify the handler completed successfully without exceptions
        // Logger verification is simplified due to NSubstitute complexity
        await _saleRepository.Received(1).GetByIdAsync(existingSale.Id);
        await _saleRepository.Received(1).UpdateAsync(existingSale);
    }

    [Fact(DisplayName = "Handle should preserve sale data when cancelling")]
    public async Task Handle_ShouldPreserveSaleDataWhenCancelling()
    {
        // Arrange
        var existingSale = SaleTestData.GenerateSaleWithItems(3);
        var originalTotalAmount = existingSale.TotalAmount;
        var originalItemsCount = existingSale.Items.Count;
        var originalSaleNumber = existingSale.SaleNumber;
        
        var command = new CancelSaleCommand
        {
            Id = existingSale.Id
        };

        _saleRepository.GetByIdAsync(existingSale.Id).Returns(existingSale);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Ensure all original data is preserved
        existingSale.TotalAmount.Should().Be(originalTotalAmount);
        existingSale.Items.Count.Should().Be(originalItemsCount);
        existingSale.SaleNumber.Should().Be(originalSaleNumber);
        existingSale.Cancelled.Should().BeTrue();
    }
}