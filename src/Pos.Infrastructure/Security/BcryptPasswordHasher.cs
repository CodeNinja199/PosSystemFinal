using Pos.Application.Interfaces;

namespace Pos.Infrastructure.Security;

// How password hashing works here:
// 1. At registration the plain password goes through bcrypt once and only the hash is stored; the plain text is never kept.
// 2. bcrypt adds a random salt and repeats its work 2^11 times (the library default), so two users with the same password get different hashes
//    and guessing is slow on purpose.
// 3. At login the plain password is checked against the stored hash with Verify; the hash is never turned back into a password.
// Learned from: https://github.com/BcryptNet/bcrypt.net (the README of the BCrypt.Net-Next package).
public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        return hashedPassword;
    }

    public bool IsPasswordCorrect(string plainPassword, string hashedPasswordFromDatabase)
    {
        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPasswordFromDatabase);

        return isPasswordCorrect;
    }
}