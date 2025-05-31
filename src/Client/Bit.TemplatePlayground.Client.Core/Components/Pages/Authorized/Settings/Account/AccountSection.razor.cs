using Bit.TemplatePlayground.Shared.Dtos.Identity;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Authorized.Settings.Account;

public partial class AccountSection
{
    [Parameter] public UserDto? User { get; set; }


    [AutoInject] private IWebAuthnService webAuthnService = default!;


    private bool showPasswordless;


    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        if (InPrerenderSession is false)
        {
            showPasswordless = await webAuthnService.IsWebAuthnAvailable();
        }
    }
}
