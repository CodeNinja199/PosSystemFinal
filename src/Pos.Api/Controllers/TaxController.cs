using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxController : ControllerBase
{
    private readonly TaxService _taxService;

    public TaxController(TaxService taxService)
    {
        _taxService = taxService;
    }

    // Any logged-in role may read the rate, because the checkout screen shows the GST before the sale is made (requirement 58).
    [Authorize]
    [HttpGet]
    public IActionResult GetTaxRate()
    {
        TaxRateResponse taxRate = _taxService.GetTaxRate();

        return Ok(taxRate);
    }
}