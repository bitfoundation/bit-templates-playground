namespace Bit.TemplatePlayground.Shared.Features.Identity.Dtos;

[DtoResourceType(typeof(AppStrings))]
public partial class ClaimDto
{
    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public static bool operator ==(ClaimDto @this, ClaimDto that)
    {
        return @this.ClaimType == that.ClaimType
             && @this.ClaimValue == that.ClaimValue;
    }

    public static bool operator !=(ClaimDto @this, ClaimDto that)
    {
        return @this.ClaimType != that.ClaimType
             || @this.ClaimValue != that.ClaimValue;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }
}
