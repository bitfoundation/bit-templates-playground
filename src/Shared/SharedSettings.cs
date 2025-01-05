namespace Bit.TemplatePlayground.Shared;

public partial class SharedSettings : IValidatableObject
{

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();


        return validationResults;
    }
}

