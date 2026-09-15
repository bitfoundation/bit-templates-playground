namespace Bit.TemplatePlayground.Client.Core.Components.Layout.Header;

public partial class Header : AppComponentBase
{
    [CascadingParameter] public BitDir? CurrentDir { get; set; }

    /// <summary>
    /// The nav panel items, flattened into the suggestions of the header search box so it jumps straight to a page.
    /// </summary>
    [Parameter] public List<BitNavItem> NavItems { get; set; } = [];


    [AutoInject] private History history = default!;


    private string? pageTitle;
    private string? pageSubtitle;
    private bool showGoBackButton;
    private List<string> searchItems = [];
    private readonly Dictionary<string, string> searchUrls = new(StringComparer.OrdinalIgnoreCase);
    private Action unsubscribePageTitleChanged = default!;

    /// <summary>
    /// The home page, with or without the culture segment (/ or /en-US).
    /// </summary>
    private bool IsHomePage
    {
        get
        {
            var path = new Uri(NavigationManager.Uri).AbsolutePath.Trim('/');

            return path.Length == 0 || string.Equals(path, CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    private string SignInUrl => $"{PageUrls.SignIn}?return-url={Uri.EscapeDataString(NavigationManager.GetRelativePath())}";
    private string SignUpUrl => $"{PageUrls.SignUp}?return-url={Uri.EscapeDataString(NavigationManager.GetRelativePath())}";


    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        unsubscribePageTitleChanged = PubSubService.Subscribe(ClientAppMessages.PAGE_DATA_CHANGED, async payload =>
        {
            (pageTitle, pageSubtitle, showGoBackButton) = ((string?, string?, bool))payload!;

            StateHasChanged();
        });
    }

    protected override async Task OnParamsSetAsync()
    {
        await base.OnParamsSetAsync();

        searchUrls.Clear();
        AddSearchItems(NavItems);
        searchItems = [.. searchUrls.Keys];
    }


    private void AddSearchItems(IEnumerable<BitNavItem> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Text) is false && string.IsNullOrWhiteSpace(item.Url) is false)
            {
                searchUrls.TryAdd(item.Text, item.Url);
            }

            if (item.ChildItems is { Count: > 0 })
            {
                AddSearchItems(item.ChildItems);
            }
        }
    }

    private void NavigateToSearchResult(string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return;

        var url = searchUrls.TryGetValue(term, out var exactUrl)
            ? exactUrl
            : searchUrls.FirstOrDefault(pair => pair.Key.Contains(term, StringComparison.OrdinalIgnoreCase)).Value;

        if (url is null) return;

        NavigationManager.NavigateTo(url);
    }

    private void OpenNavPanel()
    {
        PubSubService.Publish(ClientAppMessages.OPEN_NAV_PANEL);
    }

    private async Task GoBack()
    {
        await history.GoBack();
    }


    protected override async ValueTask DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing);

        unsubscribePageTitleChanged?.Invoke();
    }
}
