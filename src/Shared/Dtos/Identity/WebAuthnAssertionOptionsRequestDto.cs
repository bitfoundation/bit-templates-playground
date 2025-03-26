namespace Bit.TemplatePlayground.Shared.Dtos.Identity;

public partial class WebAuthnAssertionOptionsRequestDto
{
    public Guid[] UserIds { get; set; } = [];
}
