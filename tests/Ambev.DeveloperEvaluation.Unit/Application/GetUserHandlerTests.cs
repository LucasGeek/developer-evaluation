using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetUserHandler(_userRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);
        
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            Phone = "+1234567890"
        };

        var expectedResult = new GetUserResult
        {
            Id = userId,
            Email = user.Email,
            Username = user.Username
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _mapper.Map<GetUserResult>(user)
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.Equal(expectedResult.Email, result.Email);
        Assert.Equal(expectedResult.Username, result.Username);
        
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        _mapper.Received(1).Map<GetUserResult>(user);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowValidationException()
    {
        // Arrange
        var command = new GetUserCommand(Guid.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new GetUserCommand(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"User with ID {userId} not found", exception.Message);
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }
}