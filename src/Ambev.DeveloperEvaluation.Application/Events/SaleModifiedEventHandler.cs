using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.ReadModels;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IReadModelRepository<SaleReadModel> _saleReadModelRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(
        ISaleRepository saleRepository,
        IReadModelRepository<SaleReadModel> saleReadModelRepository,
        IMapper mapper,
        ILogger<SaleModifiedEventHandler> logger)
    {
        _saleRepository = saleRepository;
        _saleReadModelRepository = saleReadModelRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing SaleModifiedEvent for Sale ID: {SaleId}", notification.SaleId);

            // Get the updated sale entity from PostgreSQL
            var sale = await _saleRepository.GetByIdAsync(notification.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale not found for ID: {SaleId}", notification.SaleId);
                return;
            }

            // Map to read model
            var saleReadModel = _mapper.Map<SaleReadModel>(sale);

            // Update MongoDB read model
            await _saleReadModelRepository.UpdateAsync(saleReadModel, cancellationToken);

            _logger.LogInformation("Sale read model updated successfully for Sale ID: {SaleId}", notification.SaleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SaleModifiedEvent for Sale ID: {SaleId}", notification.SaleId);
            throw;
        }
    }
}