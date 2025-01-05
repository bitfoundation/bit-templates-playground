using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Shared.Controllers.Identity;

namespace Bit.TemplatePlayground.Client.Core.Components.Pages.Authorized.Settings;

public partial class SettingsPage
{
    protected override string? Title => Localizer[nameof(AppStrings.Settings)];
    protected override string? Subtitle => string.Empty;


    [Parameter] public string? Section { get; set; }


    [AutoInject] private IUserController userController = default!;


    private UserDto? user;
    private bool isLoading;
    private string? openedAccordion;


    protected override async Task OnInitAsync()
    {
        openedAccordion = Section?.ToLower();

        isLoading = true;

        try
        {
            user = (await PrerenderStateService.GetValue(() => HttpClient.GetFromJsonAsync("api/User/GetCurrentUser", JsonSerializerOptions.GetTypeInfo<UserDto>(), CurrentCancellationToken)))!;
        }
        finally
        {
            isLoading = false;
        }

        await base.OnInitAsync();
    }
}
