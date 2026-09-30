using Pos.Application.Dtos;
using Pos.Application.Settings;

namespace Pos.Application.Services;

// The one place that knows the GST rate and the one rule for working GST out, so the price at checkout, the receipt,
// and the rate the browser shows before the sale can never disagree (requirements 58 and 60).
public class TaxService
{
    private readonly TaxSettings _taxSettings;

    public TaxService(TaxSettings taxSettings)
    {
        _taxSettings = taxSettings;
    }

    public decimal GetGstPercentage()
    {
        return _taxSettings.GstPercentage;
    }

    public TaxRateResponse GetTaxRate()
    {
        TaxRateResponse taxRateResponse = new TaxRateResponse
        {
            GstPercentage = _taxSettings.GstPercentage
        };

        return taxRateResponse;
    }

    // Rounded to the paisa with halves going away from zero. Math.Round's own default is banker's rounding, which would
    // round Rs 0.045 down to Rs 0.04 instead of up to Rs 0.05, and requirement 60 says away from zero.
    public decimal CalculateGst(decimal subtotal)
    {
        decimal unroundedGst = subtotal * _taxSettings.GstPercentage / 100m;
        decimal gst = Math.Round(unroundedGst, 2, MidpointRounding.AwayFromZero);

        return gst;
    }
}