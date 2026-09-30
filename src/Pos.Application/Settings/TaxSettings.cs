namespace Pos.Application.Settings;

// The Tax section of appsettings.json. There is one GST rate for the whole system rather than one per store, because
// GST is set nationally and is the same in every store (requirement 58); a change of rate is a settings change.
public class TaxSettings
{
    public decimal GstPercentage { get; set; }
}