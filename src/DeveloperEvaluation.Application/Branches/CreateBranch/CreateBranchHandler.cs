using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Branches.CreateBranch;

/// <summary>
/// Handler for creating a new branch
/// </summary>
public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBranchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of CreateBranchHandler
    /// </summary>
    public CreateBranchHandler(
        IBranchRepository branchRepository,
        IMapper mapper,
        ILogger<CreateBranchHandler> logger)
    {
        _branchRepository = branchRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateBranchCommand
    /// </summary>
    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        
        _logger.LogInformation(
            "Creating branch {BranchName} at {Address}, CorrelationId: {CorrelationId}",
            request.Name, request.Address, correlationId);

        var nameExists = await _branchRepository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken);
        if (nameExists)
        {
            throw new InvalidOperationException($"Branch with name '{request.Name}' already exists");
        }

        var branch = new Branch(
            request.Name,
            request.Description,
            request.Address,
            request.City,
            request.State,
            request.PostalCode,
            request.Phone);

        await _branchRepository.CreateAsync(branch, cancellationToken);

        _logger.LogInformation(
            "Branch created successfully. BranchId: {BranchId}, Name: {BranchName}, CorrelationId: {CorrelationId}",
            branch.Id, branch.Name, correlationId);

        return branch.Id;
    }
}