using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by UserRepository in Infrastructure. Used by AuthService (register, login), ReportService (customers), and OrderService (admins for stock.low).
public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<bool> IsEmailRegisteredAsync(string email);

    Task<User> AddUserAsync(User user);

    Task<List<User>> GetCustomersAsync(int storeId);

    Task<List<User>> GetAdminsAsync(int storeId);
}