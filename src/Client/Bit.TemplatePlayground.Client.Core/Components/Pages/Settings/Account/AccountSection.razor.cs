using Bit.TemplatePlayground.Shared.Features.Identity.Dtos;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Settings.Account;

public partial class AccountSection
{
    [CascadingParameter] public UserDto? CurrentUser { get; set; }
}
