namespace Bit.TemplatePlayground.Shared;

public partial class SharedSettings : IValidatableObject
{
    /// <summary>
    /// If you are hosting the API and web client on different URLs (e.g., adminpanel-api.bitplatform.dev and adminpanel.bitplatform.dev), you must set `WebClientUrl` to your web client's address.
    /// This ensures that the API server redirects to the correct URL after social sign-ins and other similar actions.
    /// </summary>
    public string? WebClientUrl { get; set; }


    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();


        return validationResults;
    }
}

