using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ReportService _reportService;

    public CustomersController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        List<UserResponse> customers = await _reportService.GetCustomersAsync();

        return Ok(customers);
    }
}