namespace Pos.Application.Interfaces;

// Implemented by BcryptPasswordHasher in Infrastructure. Used by AuthService at registration and login.
public interface IPasswordHasher
{
    string HashPassword(string plainPassword);

    bool IsPasswordCorrect(string plainPassword, string hashedPasswordFromDatabase);
}