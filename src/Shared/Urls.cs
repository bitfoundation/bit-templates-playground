namespace Bit.TemplatePlayground.Shared;

public static partial class Urls
{
    public const string HomePage = "/";

    public const string NotFoundPage = "/not-found";

    public const string TermsPage = "/terms";

    public const string SettingsPage = "/settings";

    public const string AboutPage = "/about";

    public const string AddOrEditCategoryPage = "/add-edit-category";

    public const string CategoriesPage = "/categories";

    public const string DashboardPage = "/dashboard";

    public const string ProductsPage = "/products";


    public static readonly string[] All = typeof(Urls).GetFields()
                                                      .Where(f => f.FieldType == typeof(string))
                                                      .Select(f => f.GetValue(null)!.ToString()!)
                                                      .ToArray();
}
