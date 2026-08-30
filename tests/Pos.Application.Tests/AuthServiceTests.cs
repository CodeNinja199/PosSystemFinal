using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Enums;

namespace Pos.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ILoginTokenCreator> _loginTokenCreator;
    private readonly Mock<IStoreRepository> _storeRepository;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _loginTokenCreator = new Mock<ILoginTokenCreator>();
        _storeRepository = new Mock<IStoreRepository>();
        _authService = new AuthService(
            _userRepository.Object,
            _passwordHasher.Object,
            _loginTokenCreator.Object,
            _storeRepository.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_throws_ConflictException_when_the_email_is_already_registered()
    {
        _userRepository
            .Setup(repository => repository.IsEmailRegisteredAsync("ana@example.com"))
            .ReturnsAsync(true);
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Ana Perez", Email = "ana@example.com", Password = "secret123", StoreId = 2 };

        ConflictException exception = await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _authService.RegisterAsync(registerRequest);
        });

        string expectedMessage = "Email is already registered.";
        Assert.Equal(expectedMessage, exception.Message);
        _userRepository.Verify(repository => repository.AddUserAsync(It.IsAny<User>()), Times.Never());
    }

    [Fact]
    public async Task RegisterAsync_saves_a_customer_with_the_hashed_password_and_never_the_plain_one()
    {
        _userRepository
            .Setup(repository => repository.IsEmailRegisteredAsync("ana@example.com"))
            .ReturnsAsync(false);
        Store store = new Store { Id = 2, Name = "Airport Store" };
        _storeRepository
            .Setup(repository => repository.GetStoreByIdAsync(2))
            .ReturnsAsync(store);
        _passwordHasher
            .Setup(hasher => hasher.HashPassword("secret123"))
            .Returns("$2a$11$hashed");
        User? savedUser = null;
        _userRepository
            .Setup(repository => repository.AddUserAsync(It.IsAny<User>()))
            .Callback((User user) => { savedUser = user; })
            .ReturnsAsync((User user) => user);
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Ana Perez", Email = "ana@example.com", Password = "secret123", StoreId = 2 };

        UserResponse userResponse = await _authService.RegisterAsync(registerRequest);

        Assert.NotNull(savedUser);
        Assert.Equal("$2a$11$hashed", savedUser.PasswordHash);
        Assert.Equal(UserRole.Customer, savedUser.Role);
        Assert.Equal(2, savedUser.StoreId);
        Assert.Equal("Customer", userResponse.Role);
    }

    [Fact]
    public async Task LoginAsync_throws_UnauthorizedException_when_the_email_is_unknown()
    {
        User? noUser = null;
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("nobody@example.com"))
            .ReturnsAsync(noUser);
        LoginRequest loginRequest = new LoginRequest { Email = "nobody@example.com", Password = "secret123" };

        UnauthorizedException exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            await _authService.LoginAsync(loginRequest);
        });

        string expectedMessage = "Email or password is incorrect.";
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task LoginAsync_throws_UnauthorizedException_with_the_same_message_when_the_password_is_wrong()
    {
        User existingUser = new User { Id = 5, StoreId = 2, Email = "ana@example.com", PasswordHash = "$2a$11$hashed", Role = UserRole.Customer };
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("ana@example.com"))
            .ReturnsAsync(existingUser);
        _passwordHasher
            .Setup(hasher => hasher.IsPasswordCorrect("wrong", "$2a$11$hashed"))
            .Returns(false);
        LoginRequest loginRequest = new LoginRequest { Email = "ana@example.com", Password = "wrong" };

        UnauthorizedException exception = await Assert.ThrowsAsync<UnauthorizedException>(async () =>
        {
            await _authService.LoginAsync(loginRequest);
        });

        string expectedMessage = "Email or password is incorrect.";
        Assert.Equal(expectedMessage, exception.Message);
        _loginTokenCreator.Verify(creator => creator.CreateLoginToken(It.IsAny<User>()), Times.Never());
    }

    [Fact]
    public async Task LoginAsync_returns_the_token_and_the_user_when_the_password_is_correct()
    {
        User existingUser = new User { Id = 5, StoreId = 2, FullName = "Ana Perez", Email = "ana@example.com", PasswordHash = "$2a$11$hashed", Role = UserRole.Customer };
        _userRepository
            .Setup(repository => repository.GetUserByEmailAsync("ana@example.com"))
            .ReturnsAsync(existingUser);
        _passwordHasher
            .Setup(hasher => hasher.IsPasswordCorrect("secret123", "$2a$11$hashed"))
            .Returns(true);
        _loginTokenCreator
            .Setup(creator => creator.CreateLoginToken(existingUser))
            .Returns("token-for-ana");
        LoginRequest loginRequest = new LoginRequest { Email = "ana@example.com", Password = "secret123" };

        LoginResponse loginResponse = await _authService.LoginAsync(loginRequest);

        Assert.Equal("token-for-ana", loginResponse.Token);
        Assert.Equal("Ana Perez", loginResponse.FullName);
        Assert.Equal("Customer", loginResponse.Role);
    }
}