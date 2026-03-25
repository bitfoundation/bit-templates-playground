using Microsoft.Extensions.Caching.Memory;

namespace Bit.TemplatePlayground.Shared;

public partial class SharedSettings : IValidatableObject
{

    public MemoryCacheOptions MemoryCache { get; set; } = default!;

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();


        return validationResults;
    }
}

