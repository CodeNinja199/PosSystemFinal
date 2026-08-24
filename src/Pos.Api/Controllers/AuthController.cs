using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        UserResponse registeredUser = await _authService.RegisterAsync(registerRequest);

        return StatusCode(StatusCodes.Status201Created, registeredUser);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        LoginResponse loginResponse = await _authService.LoginAsync(loginRequest);

        return Ok(loginResponse);
    }
}