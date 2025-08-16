using Ambev.DeveloperEvaluation.Common.Security;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher;

    public BCryptPasswordHasherTests()
    {
        _hasher = new BCryptPasswordHasher();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.True(hashedPassword.StartsWith("$2a$") || hashedPassword.StartsWith("$2b$") || hashedPassword.StartsWith("$2y$"));
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hashedPassword = _hasher.HashPassword("TestPassword123!");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword(null, hashedPassword));
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ShouldThrowArgumentNullException()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword(password, null));
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var hashedPassword = _hasher.HashPassword("TestPassword123!");

        // Act
        var result = _hasher.VerifyPassword(string.Empty, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _hasher.VerifyPassword(password, string.Empty));
    }
}