using Bit.TemplatePlayground.Shared.Dtos.Identity;

namespace Bit.TemplatePlayground.Server.Api.Models.Identity;

public partial class UserSession
{
    public Guid Id { get; set; }

    public string? IP { get; set; }

    public string? Address { get; set; }

    /// <summary>
    /// <inheritdoc cref="AuthPolicies.PRIVILEGED_ACCESS"/>
    /// </summary>
    public bool Privileged { get; set; }

    /// <summary>
    /// Unix Time Seconds
    /// </summary>
    public long StartedOn { get; set; }

    /// <summary>
    /// Unix Time Seconds
    /// </summary>
    public long? RenewedOn { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }


    public string? SignalRConnectionId { get; set; }

    public UserSessionNotificationStatus NotificationStatus { get; set; }

    public string? DeviceInfo { get; set; }

    public AppPlatformType? PlatformType { get; set; }

    /// <summary>
    /// The culture selected by the user for this session.
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    /// The version of the application used for this session.
    /// </summary>
    public string? AppVersion { get; set; }
}
