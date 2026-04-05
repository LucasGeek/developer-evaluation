using DeveloperEvaluation.Application.Users.UpdateUser;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Enums;
using DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DeveloperEvaluation.Unit.Application;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserHandler> _logger;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<UpdateUserHandler>>();
        _handler = new UpdateUserHandler(_userRepository, _mapper, _logger);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "updateduser",
            Email = "updated@example.com",
            Phone = "+9876543210",
            Role = "Manager",
            Status = "Active"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com",
            Username = "olduser",
            Phone = "+1234567890"
        };

        var expectedResult = new UpdateUserResult
        {
            Id = userId,
            Username = command.Username,
            Email = command.Email
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _mapper.Map<UpdateUserResult>(Arg.Any<User>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.Equal(expectedResult.Username, result.Username);
        Assert.Equal(expectedResult.Email, result.Email);

        // Verify user properties were updated
        Assert.Equal(command.Username, existingUser.Username);
        Assert.Equal(command.Email, existingUser.Email);
        Assert.Equal(command.Phone, existingUser.Phone);
        Assert.Equal(UserRole.Manager, existingUser.Role);
        Assert.Equal(UserStatus.Active, existingUser.Status);

        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<UpdateUserResult>(existingUser);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Phone = "+1234567890",
            Role = "Customer",
            Status = "Active"
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"User with ID {userId} not found", exception.Message);
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRole_ShouldNotUpdateRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Phone = "+1234567890",
            Role = "InvalidRole",
            Status = "Active"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com",
            Username = "olduser",
            Phone = "+1234567890"
        };
        var originalRole = existingUser.Role;

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _mapper.Map<UpdateUserResult>(Arg.Any<User>())
            .Returns(new UpdateUserResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalRole, existingUser.Role); // Role should remain unchanged
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldNotUpdateStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Phone = "+1234567890",
            Role = "Customer",
            Status = "InvalidStatus"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com",
            Username = "olduser",
            Phone = "+1234567890"
        };
        var originalStatus = existingUser.Status;

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _mapper.Map<UpdateUserResult>(Arg.Any<User>())
            .Returns(new UpdateUserResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalStatus, existingUser.Status); // Status should remain unchanged
    }
}