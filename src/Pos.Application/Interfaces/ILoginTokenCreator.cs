using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by JwtLoginTokenCreator in Infrastructure. Used by AuthService after a password matches.
public interface ILoginTokenCreator
{
    string CreateLoginToken(User user);
}