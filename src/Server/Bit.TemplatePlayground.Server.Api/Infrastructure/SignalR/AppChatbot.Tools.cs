using System.ComponentModel;
using Bit.TemplatePlayground.Server.Api.Features.Identity;
using Bit.TemplatePlayground.Server.Api.Infrastructure.Services;
using Bit.TemplatePlayground.Shared;
using Bit.TemplatePlayground.Shared.Features.Diagnostic;
using Bit.TemplatePlayground.Shared.Features.Identity.Dtos;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.SignalR;
using ModelContextProtocol.Server;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.SignalR;

[McpServerToolType]
public partial class AppChatbot
{
    /// <summary>
    /// Returns the current date and time based on the user's timezone.
    /// </summary>
    [Description("Returns the current date and time based on the user's timezone.")]
    [McpServerTool(Name = nameof(GetCurrentDateTime))]
    private string GetCurrentDateTime([Required, Description("User's timezone id")] string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var userDateTime = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone);

            return $"Current date/time in user's timezone ({timeZoneId}) is {userDateTime:o}";
        }
        catch
        {
            return $"Current date/time in utc is {timeProvider.GetUtcNow():o}";
        }
    }

    /// <summary>
    /// Saves the user's email address and the conversation history for future reference.
    /// </summary>
    [Description("Saves the user's email address and the conversation history for future reference.")]
    [McpServerTool(Name = nameof(SaveUserEmailAndConversationHistory))]
    private async Task<string?> SaveUserEmailAndConversationHistory(
        [Required, Description("User's email address")] string emailAddress,
        [Required, Description("Full conversation history")] string conversationHistory)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            // Ideally, store these in a CRM or app database,
            // but for now, we'll log them!
            scope.ServiceProvider.GetRequiredService<ILogger<AIAgent>>()
                .LogError("Chat reported issue: User email: {emailAddress}, Conversation history: {conversationHistory}", emailAddress, conversationHistory);

            return "User email and conversation history saved successfully.";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to save user email and conversation history.";
        }
    }

    /// <summary>
    /// Navigates the user to a specific page within the application.
    /// </summary>
    [Description("Navigates the user to a specific page within the application. Use this tool only when the user explicitly requests to go to a particular section or feature of the app.")]
    [McpServerTool(Name = nameof(NavigateToPage))]
    private async Task<string?> NavigateToPage(
        [Required, Description("Page URL to navigate to")] string pageUrl)
    {
        await EnsureSignalRConnectionIdIsPresent();

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            _ = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.NAVIGATE_TO, pageUrl, CancellationToken.None);

            return "Navigation completed";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Navigation failed";
        }
    }

    /// <summary>
    /// Returns the list of available application pages with their relative URLs and descriptions.
    /// </summary>
    [Description("Returns the list of available application pages, each with its relative URL and a short description. Call this tool whenever the user asks to find, open or navigate to a specific page/section of the app, then use the returned relative URL (e.g. /dashboard) with the NavigateToPage tool.")]
    [McpServerTool(Name = nameof(GetAppPages))]
    private object GetAppPages()
    {
        return PageUrls.GetPages();
    }

    [Description(@"Displays the sign-in modal to the user and waits for either successful sign-in or cancellation")]
    [McpServerTool(Name = nameof(ShowSignInModal))]
    public async Task<UserDto?> ShowSignInModal()
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            await EnsureSignalRConnectionIdIsPresent();

            var accessToken = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<string>(SharedAppMessages.SHOW_SIGN_IN_MODAL, CancellationToken.None);

            var bearerTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).BearerTokenProtector;
            var accessTokenTicket = bearerTokenProtector.Unprotect(accessToken);
            var user = accessTokenTicket!.Principal;

            return await scope.ServiceProvider.GetRequiredService<AppDbContext>()
                .Users
                .Project()
                .FirstOrDefaultAsync(u => u.Id == user.GetUserId());
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return null;
        }
    }

    /// <summary>
    /// Changes the user's culture/language setting.
    /// </summary>
    [Description("Changes the user's culture/language setting. Use this tool only when the user explicitly requests to change the app language. Common LCIDs: 1033=en-US, 1065=fa-IR, 1053=sv-SE, 2057=en-GB, 1043=nl-NL, 1081=hi-IN, 2052=zh-CN, 3082=es-ES, 1036=fr-FR, 1025=ar-SA, 1031=de-DE.")]
    [McpServerTool(Name = nameof(SetApplicationCulture))]
    private async Task<string?> SetApplicationCulture(
        [Required, Description("Culture LCID (e.g., 1033 for en-US, 1065 for fa-IR)")] int cultureLcid)
    {
        await EnsureSignalRConnectionIdIsPresent();

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureLcid);

            if (CultureInfoManager.SupportedCultures.All(c => c.Culture.LCID != cultureLcid))
                return $"The requested culture is not supported. Available cultures: {string.Join(", ", CultureInfoManager.SupportedCultures.Select(c => c.Culture.NativeName))}";

            _ = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.CHANGE_CULTURE, cultureLcid, CancellationToken.None);

            return "Culture/Language changed successfully";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to change culture/language";
        }
    }

    /// <summary>
    /// Changes the user's theme preference between light and dark mode.
    /// </summary>
    [Description("Changes the user's theme preference between light and dark mode. Use this tool only when the user explicitly requests to change the app theme or appearance.")]
    [McpServerTool(Name = nameof(SetApplicationTheme))]
    private async Task<string?> SetApplicationTheme(
        [Required, Description("Theme name: 'light' or 'dark'")] string theme)
    {
        await EnsureSignalRConnectionIdIsPresent();

        if (theme != "light" && theme != "dark")
            return "Invalid theme. Use 'light' or 'dark'.";

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var themeChanged = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.CHANGE_THEME, theme, CancellationToken.None);

            return themeChanged ? $"Theme changed to {theme} successfully" : $"Theme is already set to {theme}";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to change theme";
        }
    }

    /// <summary>
    /// Retrieves the last error that occurred on the user's device from the diagnostic logs.
    /// </summary>
    [Description("Retrieves the last error that occurred on the user's device from the diagnostic logs. Use this tool when troubleshooting user-reported issues, investigating application crashes, or when the user mentions something isn't working.")]
    [McpServerTool(Name = nameof(CheckLastError))]
    private async Task<string?> CheckLastError()
    {
        await EnsureSignalRConnectionIdIsPresent();

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var lastError = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<DiagnosticLogDto?>(SharedAppMessages.UPLOAD_LAST_ERROR, CancellationToken.None);

            if (lastError is null)
                return "No errors found in the diagnostic logs.";

            return lastError.ToString();
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to retrieve error information from the device.";
        }
    }

    /// <summary>
    /// Clears application files on the user's device to fix issues.
    /// </summary>
    [Description("Clears application files on the user's device to fix issues.")]
    [McpServerTool(Name = nameof(ClearAppFiles))]
    private async Task<string?> ClearAppFiles()
    {
        await EnsureSignalRConnectionIdIsPresent();

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<DiagnosticLogDto?>(SharedAppMessages.CLEAR_APP_FILES, CancellationToken.None);

            return "App files cleared successfully on the device.";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to clear app files on the device.";
        }
    }

}
