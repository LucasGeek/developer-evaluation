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
    private readonly IBranchRepository _branchRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
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

        var customer = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {request.CustomerId} not found");
        }

        var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {request.BranchId} not found");
        }

        var saleNumber = await _saleRepository.GenerateNextSaleNumberAsync(request.BranchId);

        var sale = new Sale(
            saleNumber,
            DateTime.UtcNow,
            request.CustomerId,
            customer.Username,
            request.BranchId,
            branch.Name);

        foreach (var itemDto in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
            }

            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            if (itemDto.Quantity > 20)
            {
                throw new ArgumentException("Cannot sell more than 20 identical items");
            }

            var item = new SaleItem(
                sale.Id,
                itemDto.ProductId,
                product.Title,
                itemDto.Quantity,
                product.Price);

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

        await _saleRepository.CreateAsync(sale);

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