using System.ComponentModel;

namespace Bit.TemplatePlayground.Shared;

// The [Description] attribute defines a short, human/AI friendly summary for a page.
// Only pages that carry a [Description] are exposed by PageUrls.GetPages() / GetPagesMarkdown(),
// which feed the chatbot's GetAppPages tool and the follow-up suggestions agent (see PageUrls.Pages.cs).
public static partial class PageUrls
{
    [Description("Home page.")]
    public const string Home = "/";

    public const string NotFound = "/not-found";

    [Description("Legal terms, conditions and end-user license agreement.")]
    public const string Terms = "/terms";

    [Description("Privacy policy.")]
    public const string PrivacyPolicy = "/privacy-policy";

    [Description("User settings hub (profile, account, two-factor authentication and sessions). Requires sign-in.")]
    public const string Settings = "/settings";

    [Description("Information about the application.")]
    public const string About = "/about";


    [Description("View, create, edit and delete product categories. Requires sign-in.")]
    public const string Categories = "/categories";

    [Description("Analytics overview of key data such as categories and products. Requires sign-in.")]
    public const string Dashboard = "/dashboard";

    [Description("View, create, edit and delete products. Requires sign-in.")]
    public const string Products = "/products";

    [Description("Create a new product or edit an existing one. Requires sign-in.")]
    public const string AddOrEditProduct = "/add-edit-product";


    [Description("Manage the AI assistant's system prompts. Requires sign-in.")]
    public const string SystemPrompts = "/system-prompts";

    [Description("Manage the tenants you own. Requires sign-in.")]
    public const string ManageMyTenants = "/manage-my-tenants";

    [Description("Administer all tenants. Requires sign-in.")]
    public const string ManageAllTenants = "/manage-all-tenants";

    [Description("Manage user groups and roles. Requires sign-in.")]
    public const string Roles = "/user-groups";

    [Description("Manage users. Requires sign-in.")]
    public const string Users = "/users";

    public const string WebInteropApp = "/web-interop-app.html";
}
