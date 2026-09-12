namespace Bit.TemplatePlayground.Client.Core.Components.Layout;

public partial class MainLayout
{
    private List<BitNavItem> navPanelItems = [];

    [AutoInject] protected IStringLocalizer<AppStrings> localizer = default!;
    [AutoInject] protected IAuthorizationService authorizationService = default!;

    private async Task SetNavPanelItems(ClaimsPrincipal authUser)
    {
        navPanelItems =
        [
            new()
            {
                Text = localizer[nameof(AppStrings.Home)],
                IconName = BitIconName.Home,
                Url = PageUrls.Home,
            }
        ];

        var tenantIsSelected = await authorizationService.IsAuthorized(authUser!, AuthPolicies.TENANT_SELECTED);

        var (dashboard, manageProductCatalog) = await (authorizationService.IsAuthorized(authUser!, AppFeatures.AdminPanel.Dashboard_View),
            authorizationService.IsAuthorized(authUser!, AppFeatures.AdminPanel.ProductCatalog_Manage));

        if (tenantIsSelected is false)
        {
            dashboard = manageProductCatalog = false;
        }

        if (dashboard || manageProductCatalog)
        {
            BitNavItem adminPanelItem = new()
            {
                Text = localizer[nameof(AppStrings.AdminPanel)],
                IconName = BitIconName.Admin,
                ChildItems = []
            };

            navPanelItems.Add(adminPanelItem);

            if (dashboard)
            {
                adminPanelItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Dashboard)],
                    IconName = BitIconName.BarChartVerticalFill,
                    Url = PageUrls.Dashboard,
                });
            }

            if (manageProductCatalog)
            {
                adminPanelItem.ChildItems.AddRange(
                [
                    new()
                        {
                            Text = localizer[nameof(AppStrings.Categories)],
                            IconName = BitIconName.BuildQueue,
                            Url = PageUrls.Categories,
                        },
                        new()
                        {
                            Text = localizer[nameof(AppStrings.Products)],
                            IconName = BitIconName.Product,
                            Url = PageUrls.Products,
                        }
                ]);
            }
        }


        navPanelItems.Add(new()
        {
            Text = localizer[nameof(AppStrings.Terms)],
            IconName = BitIconName.EntityExtraction,
            Url = PageUrls.Terms,
        });

        navPanelItems.Add(new()
        {
            Text = localizer[nameof(AppStrings.About)],
            IconName = BitIconName.Info,
            Url = PageUrls.About,
        });

        var (manageRoles, manageUsers, manageAiPrompt) = await (authorizationService.IsAuthorized(authUser!, AppFeatures.Management.Roles_Manage),
            authorizationService.IsAuthorized(authUser!, AppFeatures.Management.Users_Manage),
            authorizationService.IsAuthorized(authUser!, AppFeatures.Management.SystemPrompts_Write));

        if (tenantIsSelected is false)
        {
            manageRoles = manageUsers = manageAiPrompt = false;
        }

        var manageTenantsGlobally = await authorizationService.IsAuthorized(authUser!, AppFeatures.Management.Tenants_Manage_Global);

        if (manageRoles || manageUsers || manageAiPrompt
            || manageTenantsGlobally
            )
        {
            BitNavItem managementItem = new()
            {
                Text = localizer[nameof(AppStrings.Management)],
                IconName = BitIconName.SettingsSecure,
                ChildItems = []
            };

            navPanelItems.Add(managementItem);

            if (manageRoles)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.UserGroups)],
                    IconName = BitIconName.WorkforceManagement,
                    Url = PageUrls.Roles,
                });
            }

            if (manageUsers)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Users)],
                    IconName = BitIconName.SecurityGroup,
                    Url = PageUrls.Users,
                });
            }

            if (manageAiPrompt)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.SystemPromptsTitle)],
                    IconName = BitIconName.TextDocumentSettings,
                    Url = PageUrls.SystemPrompts,
                });
            }

            if (manageTenantsGlobally)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.ManageAllTenants)],
                    IconName = BitIconName.Org,
                    Url = PageUrls.ManageAllTenants,
                });
            }
        }

        if (authUser.IsAuthenticated())
        {
            navPanelItems.Add(new()
            {
                Text = localizer[nameof(AppStrings.Settings)],
                IconName = BitIconName.Equalizer,
                Url = PageUrls.Settings,
                AdditionalUrls =
                [
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Profile}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Account}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Tfa}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Sessions}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.UpgradeAccount}",
                ]
            });
        }
    }
}
