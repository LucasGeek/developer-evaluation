using MediatR;
using AutoMapper;
using DeveloperEvaluation.Domain.Events;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.ORM.MongoDB.Repositories;
using DeveloperEvaluation.ORM.MongoDB.ReadModels;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Events;

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

            var sale = await _saleRepository.GetByIdAsync(notification.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale not found for ID: {SaleId}", notification.SaleId);
                return;
            }

            var saleReadModel = _mapper.Map<SaleReadModel>(sale);

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