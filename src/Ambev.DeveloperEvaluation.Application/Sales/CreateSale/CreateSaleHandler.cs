using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _eventBus = eventBus;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        
        _logger.LogInformation(
            "Creating sale for Branch {BranchId}, Customer {CustomerId}, CorrelationId: {CorrelationId}",
            request.BranchId, request.CustomerId, correlationId);

        // Validate customer exists
        var customer = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {request.CustomerId} not found");
        }

        // Generate sale number
        var saleNumber = await _saleRepository.GenerateNextSaleNumberAsync(request.BranchId);

        // Create sale entity with customer description from database
        var sale = new Sale(
            saleNumber,
            DateTime.UtcNow,
            request.CustomerId,
            customer.Username, // Use actual customer name from database
            request.BranchId,
            "Main Branch"); // This should come from a Branch repository in a real app

        // Process each item with validation and business rules
        foreach (var itemDto in request.Items)
        {
            // Validate product exists and get current price
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
            }

            // Apply business rules for quantity
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            if (itemDto.Quantity > 20)
            {
                throw new ArgumentException("Cannot sell more than 20 identical items");
            }

            // Create sale item with actual product data
            var item = new SaleItem(
                sale.Id,
                itemDto.ProductId,
                product.Title, // Use actual product title from database
                itemDto.Quantity,
                product.Price); // Use actual product price from database

            // Apply discount based on business rules (handled by the entity)
            item.ApplyDiscount();
            
            if (item.Discount > 0)
            {
                var discountPercentage = itemDto.Quantity >= 10 ? 20 : 10;
                _logger.LogInformation(
                    "Applied {DiscountPercentage}% discount to product {ProductId} for quantity {Quantity}. Discount amount: {DiscountAmount}",
                    discountPercentage, itemDto.ProductId, itemDto.Quantity, item.Discount);
            }

            sale.AddItem(item);
        }

        // Persist to database
        await _saleRepository.CreateAsync(sale);

        // Publish SaleCreated event
        var saleCreatedEvent = new SaleCreatedEvent
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            CustomerId = sale.CustomerId,
            CustomerDescription = sale.CustomerDescription,
            BranchId = sale.BranchId,
            BranchDescription = sale.BranchDescription,
            TotalAmount = sale.TotalAmount,
            CreatedAt = sale.CreatedAt,
            ItemCount = sale.Items.Count
        };

        await _eventBus.PublishAsync(saleCreatedEvent);

        _logger.LogInformation(
            "Sale created successfully. SaleId: {SaleId}, SaleNumber: {SaleNumber}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}",
            sale.Id, sale.SaleNumber, sale.TotalAmount, correlationId);

        return sale.Id;
    }
}