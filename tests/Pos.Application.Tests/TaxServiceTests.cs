using Pos.Application.Dtos;
using Pos.Application.Services;
using Pos.Application.Settings;

namespace Pos.Application.Tests;

public class TaxServiceTests
{
    [Fact]
    public void GetTaxRate_returns_the_one_configured_gst_rate()
    {
        TaxService taxService = new TaxService(new TaxSettings { GstPercentage = 18m });

        TaxRateResponse taxRate = taxService.GetTaxRate();

        Assert.Equal(18m, taxRate.GstPercentage);
    }

    [Theory]
    [InlineData(200, 36)]
    [InlineData(450, 81)]
    [InlineData(0.25, 0.05)]
    [InlineData(59.99, 10.80)]
    public void CalculateGst_takes_18_percent_rounded_to_the_paisa_with_halves_away_from_zero(double subtotal, double expectedGst)
    {
        TaxService taxService = new TaxService(new TaxSettings { GstPercentage = 18m });

        decimal gst = taxService.CalculateGst((decimal)subtotal);

        Assert.Equal((decimal)expectedGst, gst);
    }

    [Fact]
    public void CalculateGst_is_zero_when_the_rate_is_zero()
    {
        TaxService taxService = new TaxService(new TaxSettings { GstPercentage = 0m });

        decimal gst = taxService.CalculateGst(450m);

        Assert.Equal(0m, gst);
    }
}