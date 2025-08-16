using Ambev.DeveloperEvaluation.Common.Security;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public class JwtTokenGeneratorTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenGenerator _tokenGenerator;

    public JwtTokenGeneratorTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        
        // Setup configuration values
        _configuration["Jwt:SecretKey"].Returns("ThisIsAVeryLongSecretKeyForTestingPurposesAndItShouldBeAtLeast32Characters");
        _configuration["Jwt:Issuer"].Returns("TestIssuer");
        _configuration["Jwt:Audience"].Returns("TestAudience");
        _configuration["Jwt:ExpirationInMinutes"].Returns("60");
        
        _tokenGenerator = new JwtTokenGenerator(_configuration);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Id.Returns(Guid.NewGuid().ToString());
        user.Username.Returns("testuser");
        user.Role.Returns("Customer");

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify the token can be parsed
        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(token);
        Assert.True(canRead);
        
        // Verify token is parseable and contains data
        var jsonToken = handler.ReadJwtToken(token);
        Assert.NotNull(jsonToken);
        Assert.True(jsonToken.Claims.Any());
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_ShouldReturnDifferentTokens()
    {
        // Arrange
        var user1 = Substitute.For<IUser>();
        user1.Id.Returns(Guid.NewGuid().ToString());
        user1.Username.Returns("user1");
        user1.Role.Returns("Customer");
        
        var user2 = Substitute.For<IUser>();
        user2.Id.Returns(Guid.NewGuid().ToString());
        user2.Username.Returns("user2");
        user2.Role.Returns("Manager");

        // Act
        var token1 = _tokenGenerator.GenerateToken(user1);
        var token2 = _tokenGenerator.GenerateToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_WithValidParameters_ShouldIncludeExpirationTime()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Id.Returns(Guid.NewGuid().ToString());
        user.Username.Returns("testuser");
        user.Role.Returns("Customer");
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.True(jsonToken.ValidTo > beforeGeneration.AddHours(7)); // Should be close to 8 hours
        Assert.True(jsonToken.ValidTo < DateTime.UtcNow.AddHours(9)); // Should not exceed expected time
    }

    [Fact]
    public void GenerateToken_WithNullUser_ShouldThrowNullReferenceException()
    {
        // Arrange
        IUser user = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _tokenGenerator.GenerateToken(user));
    }
}