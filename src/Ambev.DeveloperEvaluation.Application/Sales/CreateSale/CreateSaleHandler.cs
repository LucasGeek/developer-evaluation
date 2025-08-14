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
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
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

        // Generate sale number
        var saleNumber = await _saleRepository.GenerateNextSaleNumberAsync(request.BranchId);

        // Create sale entity
        var sale = new Sale(
            saleNumber,
            request.Date,
            request.CustomerId,
            request.CustomerDescription,
            request.BranchId,
            request.BranchDescription);

        // Add items with business rules applied
        foreach (var itemDto in request.Items)
        {
            var item = new SaleItem(
                sale.Id,
                itemDto.ProductId,
                itemDto.ProductDescription,
                itemDto.Quantity,
                itemDto.UnitPrice);

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