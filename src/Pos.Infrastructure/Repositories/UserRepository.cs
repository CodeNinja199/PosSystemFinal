using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Enums;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.Repositories;

// Registered as scoped in Program.cs because it holds the scoped PosDbContext.
public class UserRepository : IUserRepository
{
    private readonly PosDbContext _context;

    public UserRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        User? userFromDatabase = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        return userFromDatabase;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        User? userFromDatabase = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == userId);

        return userFromDatabase;
    }

    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        bool isEmailRegistered = await _context.Users
            .AnyAsync(user => user.Email == email);

        return isEmailRegistered;
    }

    public async Task<User> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<List<User>> GetCustomersAsync()
    {
        List<User> customersFromDatabase = await _context.Users
            .Where(user => user.Role == UserRole.Customer)
            .OrderBy(user => user.FullName)
            .ToListAsync();

        return customersFromDatabase;
    }
}