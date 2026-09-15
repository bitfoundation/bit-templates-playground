namespace Bit.TemplatePlayground.Client.Core.Components.Common;

public partial class ProductImage
{
    [Parameter] public string? Src { get; set; }
    [Parameter] public string? Alt { get; set; }
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? Class { get; set; }

    public const string PlaceholderSrc = "_content/Bit.TemplatePlayground.Client.Core/images/car_placeholder.png";

    private string EffectiveSrc => Src ?? PlaceholderSrc;
}
