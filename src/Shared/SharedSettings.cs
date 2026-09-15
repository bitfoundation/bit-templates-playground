
namespace Bit.TemplatePlayground.Shared;

public partial class SharedSettings : IValidatableObject
{
    public ApplicationInsightsOptions? ApplicationInsights { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();

        if (ApplicationInsights is not null)
        {
            Validator.TryValidateObject(ApplicationInsights, new ValidationContext(ApplicationInsights), validationResults, true);
        }

        return validationResults;
    }
}
