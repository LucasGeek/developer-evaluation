using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UpdateUserHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", request.Id);

        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found for update", request.Id);
            throw new ArgumentException($"User with ID {request.Id} not found");
        }

        // Update user properties
        existingUser.Username = request.Username;
        existingUser.Email = request.Email;
        existingUser.Phone = request.Phone;

        // Parse and set Role
        if (Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            existingUser.Role = userRole;

        // Parse and set Status
        if (Enum.TryParse<UserStatus>(request.Status, true, out var userStatus))
            existingUser.Status = userStatus;

        var updatedUser = await _userRepository.UpdateAsync(existingUser, cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully", updatedUser.Id);

        return _mapper.Map<UpdateUserResult>(updatedUser);
    }
}