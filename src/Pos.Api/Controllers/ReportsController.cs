using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetTodaysSalesSummary()
    {
        SalesSummaryResponse salesSummary = await _reportService.GetTodaysSalesSummaryAsync();

        return Ok(salesSummary);
    }
}