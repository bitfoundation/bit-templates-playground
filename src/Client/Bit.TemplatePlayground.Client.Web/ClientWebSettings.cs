using Bit.TemplatePlayground.Client.Core;

namespace Bit.TemplatePlayground.Client.Web;

public class ClientWebSettings : ClientCoreSettings
{

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = base.Validate(validationContext).ToList();


        return validationResults;
    }
}

