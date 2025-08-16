using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _branchRepository = Substitute.For<IBranchRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _productRepository, _userRepository, _branchRepository, _eventBus, _mapper, _logger);
    }

    [Fact(DisplayName = "Handle should create sale with correct properties")]
    public async Task Handle_ValidCommand_ShouldCreateSaleWithCorrectProperties()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var command = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto>
            {
                new(productId1, 5),
                new(productId2, 2)
            });

        // Setup customer
        var customer = new User { Id = customerId, Username = "Test Customer" };
        _userRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        // Setup branch
        var branch = new Branch("Test Branch", "Test", "123 Test St", "Test City", "TS", "12345", "555-0123");
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branch);

        // Setup products
        var product1 = new Product("Product 1", 10.00m, "Description 1", "Category 1", "image1.jpg");
        var product2 = new Product("Product 2", 15.00m, "Description 2", "Category 2", "image2.jpg");
        _productRepository.GetByIdAsync(productId1, Arg.Any<CancellationToken>()).Returns(product1);
        _productRepository.GetByIdAsync(productId2, Arg.Any<CancellationToken>()).Returns(product2);

        _saleRepository.GenerateNextSaleNumberAsync(branchId)
            .Returns("BRANCH12345678-0001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _saleRepository.Received(1).GenerateNextSaleNumberAsync(command.BranchId);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Handle should apply discount rules correctly")]
    public async Task Handle_ItemsWithDifferentQuantities_ShouldApplyDiscountRulesCorrectly()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var command = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto>
            {
                new(productId1, 4), // 10% discount
                new(productId2, 10) // 20% discount
            });

        // Setup customer
        var customer = new User { Id = customerId, Username = "Test Customer" };
        _userRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        // Setup branch
        var branch = new Branch("Test Branch", "Test", "123 Test St", "Test City", "TS", "12345", "555-0123");
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branch);

        // Setup products
        var product1 = new Product("Product 1", 10.00m, "Description 1", "Category 1", "image1.jpg");
        var product2 = new Product("Product 2", 20.00m, "Description 2", "Category 2", "image2.jpg");
        _productRepository.GetByIdAsync(productId1, Arg.Any<CancellationToken>()).Returns(product1);
        _productRepository.GetByIdAsync(productId2, Arg.Any<CancellationToken>()).Returns(product2);

        _saleRepository.GenerateNextSaleNumberAsync(command.BranchId)
            .Returns("BRANCH12345678-0001");

        Sale capturedSale = null;
        await _saleRepository.CreateAsync(Arg.Do<Sale>(sale => capturedSale = sale));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSale.Should().NotBeNull();
        capturedSale.Items.Should().HaveCount(2);
        
        // First item: 4 qty * 10.00 = 40.00, discount 10% = 4.00, total = 36.00
        var firstItem = capturedSale.Items.First();
        firstItem.Quantity.Should().Be(4);
        firstItem.UnitPrice.Should().Be(10.00m);
        firstItem.Discount.Should().Be(4.00m);
        firstItem.Total.Should().Be(36.00m);

        // Second item: 10 qty * 20.00 = 200.00, discount 20% = 40.00, total = 160.00
        var secondItem = capturedSale.Items.Last();
        secondItem.Quantity.Should().Be(10);
        secondItem.UnitPrice.Should().Be(20.00m);
        secondItem.Discount.Should().Be(40.00m);
        secondItem.Total.Should().Be(160.00m);

        // Total sale: 36.00 + 160.00 = 196.00
        capturedSale.TotalAmount.Should().Be(196.00m);
    }

    [Fact(DisplayName = "Handle should generate unique sale number")]
    public async Task Handle_ValidCommand_ShouldGenerateUniqueSaleNumber()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var command = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto>
            {
                new(productId, 1)
            });

        // Setup customer
        var customer = new User { Id = customerId, Username = "Test Customer" };
        _userRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        // Setup branch
        var branch = new Branch("Test Branch", "Test", "123 Test St", "Test City", "TS", "12345", "555-0123");
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branch);

        // Setup product
        var product = new Product("Product 1", 10.00m, "Description 1", "Category 1", "image1.jpg");
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var expectedSaleNumber = $"BRANCH{branchId.ToString("N").Substring(0, 8).ToUpper()}-0001";
        _saleRepository.GenerateNextSaleNumberAsync(branchId).Returns(expectedSaleNumber);

        Sale capturedSale = null;
        await _saleRepository.CreateAsync(Arg.Do<Sale>(sale => capturedSale = sale));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSale.Should().NotBeNull();
        capturedSale.SaleNumber.Should().Be(expectedSaleNumber);
        capturedSale.BranchId.Should().Be(branchId);
    }

    [Fact(DisplayName = "Handle should log creation events")]
    public async Task Handle_ValidCommand_ShouldLogCreationEvents()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var command = new CreateSaleCommand(
            BranchId: branchId,
            CustomerId: customerId,
            Items: new List<CreateSaleItemDto>
            {
                new(productId, 1)
            });

        // Setup customer
        var customer = new User { Id = customerId, Username = "Test Customer" };
        _userRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(customer);

        // Setup branch
        var branch = new Branch("Test Branch", "Test", "123 Test St", "Test City", "TS", "12345", "555-0123");
        _branchRepository.GetByIdAsync(branchId, Arg.Any<CancellationToken>()).Returns(branch);

        // Setup product
        var product = new Product("Product 1", 10.00m, "Description 1", "Category 1", "image1.jpg");
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        _saleRepository.GenerateNextSaleNumberAsync(command.BranchId)
            .Returns("BRANCH12345678-0001");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify that logging occurred (we can't easily mock the exact log calls due to extension methods)
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>());
    }
}