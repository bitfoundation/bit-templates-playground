using System.ComponentModel;
using Bit.TemplatePlayground.Server.Api.Features.Identity;
using Bit.TemplatePlayground.Shared.Features.Chatbot;
using Bit.TemplatePlayground.Shared.Features.Diagnostic;
using Microsoft.Agents.AI;

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
    /// Shows the user a card in the chat for leaving their contact details, so a human operator can follow up.
    /// No [McpServerTool]: the card needs the app's chat panel on the other end of the SignalR connection.
    /// </summary>
    [Description("Shows the user a form inside the chat for leaving their contact details, so a human operator can follow up on an issue you could not resolve. Use it instead of asking for an email address or phone number yourself.")]
    private async Task<string?> RequestHumanFollowUp(
        [Required, Description("A summary of the conversation so far in at most three sentences, written in the user's language")] string conversationSummary,
        CancellationToken cancellationToken = default)
    {
        var shown = await ShowCard(new()
        {
            ComponentType = AiChatCardComponents.HumanFollowUp,
            Data = { ["ConversationSummary"] = conversationSummary },
            RawMarkdown = $"Showed a form for leaving contact details, so a human operator can follow up on: {conversationSummary}"
        }, cancellationToken);

        return shown
            ? "The contact form was shown to the user in the chat. They fill in their contact details there, so do not ask for them yourself."
            : "Failed to show the contact form.";
    }

    /// <summary>Sent rather than invoked: nothing waits for the panel. No [McpServerTool]: the buttons need the chat panel.</summary>
    [Description("Shows the user exactly 3 things they might want to ask or do next, as buttons to tap under your answer. Each is under 60 characters, worded as the user would say it and in the language you answer in.")]
    private async Task<string> ShowFollowUpSuggestions(
        [Required, Description("The suggestions, most useful first")] string[] suggestions,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .SendAsync(SharedAppMessages.SHOW_AI_CHAT_SUGGESTIONS, suggestions, cancellationToken);

            return "The suggestions are shown to the user.";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to show the suggestions.";
        }
    }

    /// <summary>
    /// Navigates the user to a specific page within the application.
    /// </summary>
    [Description("Navigates the user to a specific page within the application. Use this tool only when the user explicitly requests to go to a particular section or feature of the app.")]
    private async Task<string?> NavigateToPage(
        [Required, Description("Page URL to navigate to")] string pageUrl,
        CancellationToken cancellationToken = default)
    {
        if (Uri.IsAppRelativeUrl(pageUrl) is false)
            return "Invalid page url. Only app relative urls such as /dashboard are allowed.";

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            _ = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.NAVIGATE_TO, pageUrl, cancellationToken);

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
    public async Task<UserDto?> ShowSignInModal(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var accessToken = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<string>(SharedAppMessages.SHOW_SIGN_IN_MODAL, cancellationToken);

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
    private async Task<string?> SetApplicationCulture(
        [Required, Description("Culture LCID (e.g., 1033 for en-US, 1065 for fa-IR)")] int cultureLcid,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureLcid);

            if (CultureInfoManager.SupportedCultures.All(c => c.Culture.LCID != cultureLcid))
                return $"The requested culture is not supported. Available cultures: {string.Join(", ", CultureInfoManager.SupportedCultures.Select(c => c.Culture.NativeName))}";

            _ = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.CHANGE_CULTURE, cultureLcid, cancellationToken);

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
    private async Task<string?> SetApplicationTheme(
        [Required, Description("Theme name: 'light' or 'dark'")] string theme,
        CancellationToken cancellationToken = default)
    {
        if (theme != "light" && theme != "dark")
            return "Invalid theme. Use 'light' or 'dark'.";

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var themeChanged = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.CHANGE_THEME, theme, cancellationToken);

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
    private async Task<string?> CheckLastError(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var lastError = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<DiagnosticLogDto?>(SharedAppMessages.UPLOAD_LAST_ERROR, cancellationToken);

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

    /// <summary>Clears application files on the user's device to fix issues, only once the user approves on screen (See <see cref="AwaitCard"/>).</summary>
    [Description("Clears the app's files on the user's device - local data, cache and storage - to fix corrupted local state; it also signs the user out, deletes this conversation and restarts the app. It asks the user to approve on their screen and does nothing without that approval, so don't ask for permission in the conversation yourself.")]
    private async Task<string?> ClearAppFiles(CancellationToken cancellationToken = default)
    {
        var decision = await AwaitCard(new()
        {
            ComponentType = AiChatCardComponents.UserApproval,
            Data = { ["Action"] = nameof(ClearAppFiles) },
            RawMarkdown = "Asked the user to approve clearing the app's files on this device, which signs them out, deletes this conversation and restarts the app."
        }, cancellationToken);

        if (decision is AiChatCardDecision.Declined)
            return "The user declined, so nothing was cleared. Carry on another way, and don't offer it again unless they bring it up.";

        if (decision is not AiChatCardDecision.Approved)
            return "The approval went unanswered, so nothing was cleared. Don't ask again on your own.";

        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            var cleared = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.CLEAR_APP_FILES, cancellationToken);

            return cleared
                ? "The user approved, and the app files are being cleared: the app signs out, deletes this conversation and restarts."
                : "Failed to clear app files on the device.";
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return "Failed to clear app files on the device.";
        }
    }

    /// <summary>Shows a card in the conversation and keeps what it showed in the history. False when it could not be shown.</summary>
    private async Task<bool> ShowCard(AiChatCard card, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            SignCard(card);

            var shown = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<bool>(SharedAppMessages.SHOW_AI_CHAT_CARD, card, cancellationToken);

            if (shown)
            {
                RememberCard(card, cancellationToken);
            }

            return shown;
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return false;
        }
    }

    /// <summary>Shows a card and waits for the user's decision. Fails closed: an error or a cancellation is NoAnswer.</summary>
    private async Task<string> AwaitCard(AiChatCard card, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        try
        {
            SignCard(card);
            RememberCard(card, cancellationToken);

            var decision = await scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>()
                .Clients.Client(signalRConnectionId!)
                .InvokeAsync<string?>(SharedAppMessages.AWAIT_AI_CHAT_CARD, card, cancellationToken);

            return decision is AiChatCardDecision.Approved or AiChatCardDecision.Declined ? decision : AiChatCardDecision.NoAnswer;
        }
        catch (Exception exp)
        {
            serviceProvider.GetRequiredService<ApiServerExceptionHandler>().Handle(exp);
            return AiChatCardDecision.NoAnswer;
        }
    }

    /// <summary>Signed like an answer, so the panel can resend it as one. A voice call's cards stay unsigned, like its answers.</summary>
    private void SignCard(AiChatCard card)
    {
        if (isVoiceCall is false)
        {
            card.Signature = answerSigner.Sign(card.RawMarkdown!);
        }
    }

    /// <summary>Adds the card to the text chat's history as the panel resends it; not once its turn was cancelled.</summary>
    private void RememberCard(AiChatCard card, CancellationToken cancellationToken)
    {
        if (isVoiceCall || cancellationToken.IsCancellationRequested) return;

        lock (historyLock)
        {
            chatMessages.Add(new(ChatRole.Assistant, card.RawMarkdown));
        }
    }

}
