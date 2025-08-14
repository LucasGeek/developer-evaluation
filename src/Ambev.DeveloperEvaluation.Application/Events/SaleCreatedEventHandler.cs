using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.ReadModels;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IReadModelRepository<SaleReadModel> _saleReadModelRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(
        ISaleRepository saleRepository,
        IReadModelRepository<SaleReadModel> saleReadModelRepository,
        IMapper mapper,
        ILogger<SaleCreatedEventHandler> logger)
    {
        _saleRepository = saleRepository;
        _saleReadModelRepository = saleReadModelRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing SaleCreatedEvent for Sale ID: {SaleId}", notification.SaleId);

            // Get the full sale entity from PostgreSQL
            var sale = await _saleRepository.GetByIdAsync(notification.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale not found for ID: {SaleId}", notification.SaleId);
                return;
            }

            // Map to read model
            var saleReadModel = _mapper.Map<SaleReadModel>(sale);

            // Save to MongoDB for read operations
            await _saleReadModelRepository.CreateAsync(saleReadModel, cancellationToken);

            _logger.LogInformation("Sale read model created successfully for Sale ID: {SaleId}", notification.SaleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SaleCreatedEvent for Sale ID: {SaleId}", notification.SaleId);
            throw;
        }
    }
}