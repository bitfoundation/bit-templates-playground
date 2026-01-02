using Bit.TemplatePlayground.Client.Core.Components;

namespace Bit.TemplatePlayground.Client.Core.Services;

/// <summary>
/// This class is located in the Client.Core project to define
/// shared app messages used for messaging between Blazor components, JavaScript and Web Service Worker.
/// </summary>
public partial class ClientAppMessages
    : SharedAppMessages
{
    /// <summary>
    /// A publisher that sends this message announces that the subscriber should show a snack bar message.
    /// </summary>
    public const string SHOW_SNACK = nameof(SHOW_SNACK);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should show a modal.
    /// </summary>
    public const string SHOW_MODAL = nameof(SHOW_MODAL);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should close the modal.
    /// </summary>
    public const string CLOSE_MODAL = nameof(CLOSE_MODAL);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should open the navigation panel.
    /// </summary>
    public const string OPEN_NAV_PANEL = nameof(OPEN_NAV_PANEL);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should close the navigation panel.
    /// </summary>
    public const string CLOSE_NAV_PANEL = nameof(CLOSE_NAV_PANEL);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should navigate to a specific page.
    /// </summary>
    public const string NAVIGATE_TO = nameof(NAVIGATE_TO);

    /// <summary>
    /// A publisher that sends this message announces that the subscriber should show the diagnostic modal.
    /// </summary>
    public const string SHOW_DIAGNOSTIC_MODAL = nameof(SHOW_DIAGNOSTIC_MODAL);



    /// <summary>
    /// A publisher that sends this message announces that the subscriber should force the app to check for updates and install them.
    /// </summary>
    public const string FORCE_UPDATE = nameof(FORCE_UPDATE);





    /// <summary>
    /// A publisher that publishes this message notifies that the app theme has changed.
    /// </summary>
    public const string THEME_CHANGED = nameof(THEME_CHANGED);

    /// <summary>
    /// A publisher that publishes this message notifies that the app culture has changed.
    /// </summary>
    public const string CULTURE_CHANGED = nameof(CULTURE_CHANGED);

    /// <summary>
    /// A publisher that publishes this message notifies that the online status of the app has changed.
    /// <inheritdoc cref="Parameters.IsOnline"/>
    /// </summary>
    public const string IS_ONLINE_CHANGED = nameof(IS_ONLINE_CHANGED);

    /// <summary>
    /// A publisher that publishes this message notifies that the page data has changed.
    /// </summary>
    public const string PAGE_DATA_CHANGED = nameof(PAGE_DATA_CHANGED);

    /// <summary>
    /// A publisher that publishes this message notifies that the route data has been updated.
    /// </summary>
    public const string ROUTE_DATA_UPDATED = nameof(ROUTE_DATA_UPDATED);


    /// <summary>
    /// A publisher that publishes this message notifies that the user's external sign-in process has completed.
    /// When a user completes external sign-in in a separate window, this message is published to notify the app.
    /// </summary>
    public const string EXTERNAL_SIGN_IN_CALLBACK = nameof(EXTERNAL_SIGN_IN_CALLBACK);
}
