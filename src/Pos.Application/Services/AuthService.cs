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

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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