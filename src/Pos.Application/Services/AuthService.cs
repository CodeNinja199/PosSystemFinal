using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Enums;

namespace Pos.Application.Services;

// Called by AuthController. Holds the account rules; hashing comes from IPasswordHasher, storage from IUserRepository.
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILoginTokenCreator _loginTokenCreator;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ILoginTokenCreator loginTokenCreator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _loginTokenCreator = loginTokenCreator;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest registerRequest)
    {
        bool isEmailRegistered = await _userRepository.IsEmailRegisteredAsync(registerRequest.Email);
        if (isEmailRegistered)
        {
            throw new ConflictException("Email is already registered.");
        }

        string hashedPassword = _passwordHasher.HashPassword(registerRequest.Password);

        User newUser = new User
        {
            FullName = registerRequest.FullName,
            Email = registerRequest.Email,
            PasswordHash = hashedPassword,
            Role = UserRole.Customer,
            RegisteredAt = DateTime.UtcNow
        };
        User savedUser = await _userRepository.AddUserAsync(newUser);

        UserResponse userResponse = MapUserToResponse(savedUser);

        return userResponse;
    }

    // The same message for an unknown email and a wrong password, so a caller cannot learn which emails exist.
    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        User? userFromRepository = await _userRepository.GetUserByEmailAsync(loginRequest.Email);
        if (userFromRepository == null)
        {
            throw new UnauthorizedException("Email or password is incorrect.");
        }

        bool isPasswordCorrect = _passwordHasher.IsPasswordCorrect(loginRequest.Password, userFromRepository.PasswordHash);
        if (isPasswordCorrect == false)
        {
            throw new UnauthorizedException("Email or password is incorrect.");
        }

        string token = _loginTokenCreator.CreateLoginToken(userFromRepository);

        LoginResponse loginResponse = new LoginResponse
        {
            Token = token,
            FullName = userFromRepository.FullName,
            Role = userFromRepository.Role.ToString()
        };

        return loginResponse;
    }

    private static UserResponse MapUserToResponse(User user)
    {
        UserResponse userResponse = new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RegisteredAt = user.RegisteredAt
        };

        return userResponse;
    }
}