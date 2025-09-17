using Bit.TemplatePlayground.Client.Core;

namespace Bit.TemplatePlayground.Client.Web;

public class ClientWebSettings : ClientCoreSettings
{
    public AdsPushVapidOptions? AdsPushVapid { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = base.Validate(validationContext).ToList();

        if (AdsPushVapid is not null)
        {
            Validator.TryValidateObject(AdsPushVapid, new ValidationContext(AdsPushVapid), validationResults, true);

            if (AppEnvironment.IsDevelopment() is false && AdsPushVapid.PublicKey is "BDSNUvuIISD8NQVByQANEtZ2foKaENIcIGUxsiQs9kDz11fQik8c9WeiMwUHs3iTgNNH4nvXioNQIEsn4OAjTKc")
            {
                validationResults.Add(new ValidationResult("Please set your own AdsPushVapid.PublicKey in Client.Core's appsettings.json"));
            }
        }

        return validationResults;
    }
}

/// <summary>
/// https://github.com/adessoTurkey-dotNET/AdsPush
/// </summary>
public class AdsPushVapidOptions
{
    /// <summary>
    /// Web push's vapid. More info at https://tools.reactpwa.com/vapid
    /// </summary>
    [Required]
    public string PublicKey { get; set; } = default!;
}
