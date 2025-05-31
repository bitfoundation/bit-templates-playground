using Bit.TemplatePlayground.Shared.Dtos.Identity;
using Bit.TemplatePlayground.Server.Api.Models.Identity;
using Bit.TemplatePlayground.Shared.Controllers.Identity;
using Microsoft.AspNetCore.SignalR;
using Bit.TemplatePlayground.Server.Api.SignalR;

namespace Bit.TemplatePlayground.Server.Api.Controllers.Identity;

[ApiController, Route("api/[controller]/[action]")]
[Authorize(Policy = AppFeatures.Management.ManageUsers)]
public partial class UserManagementController : AppControllerBase, IUserManagementController
{
    [AutoInject] private UserManager<User> userManager = default!;
    [AutoInject] private IHubContext<AppHub> appHubContext = default!;
    [AutoInject] private ServerApiSettings serverApiSettings = default!;


    [HttpGet, EnableQuery]
    public IQueryable<UserDto> GetAllUsers()
    {
        return userManager.Users.Project();
    }

    [HttpGet]
    public async Task<int> GetOnlineUsersCount(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return await DbContext.Users.CountAsync(u => u.Sessions.Any(us => (now - (us.RenewedOn ?? us.StartedOn)) < serverApiSettings.Identity.BearerTokenExpiration.TotalSeconds), cancellationToken);
    }

    [HttpGet("{userId}"), EnableQuery]
    public IQueryable<UserSessionDto> GetUserSessions(Guid userId)
    {
        return DbContext.UserSessions.Where(us => us.UserId == userId).Project();
    }

    [HttpPost("{userId}")]
    [Authorize(Policy = AuthPolicies.ELEVATED_ACCESS)]
    public async Task Delete(Guid userId, CancellationToken cancellationToken)
    {
        if (User.GetUserId() == userId)
            throw new BadRequestException(Localizer[nameof(AppStrings.UserCantRemoveItselfErrorMessage)]);

        var user = await GetUserByIdAsync(userId, cancellationToken);

        if (await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
        {
            if (User.IsInRole(AppRoles.SuperAdmin) is false)
                throw new BadRequestException(Localizer[nameof(AppStrings.UserCantRemoveSuperAdminErrorMessage)]);
        }
        
        var userSessionConnectionIds = await DbContext.UserSessions.Where(us => us.UserId == userId && us.SignalRConnectionId != null)
                                                                   .Select(us => us.SignalRConnectionId!)
                                                                   .ToListAsync(cancellationToken);

        await DbContext.UserSessions.Where(us => us.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await userManager.DeleteAsync(user);

        foreach (var id in userSessionConnectionIds)
        {
            await RevokeSession(id, cancellationToken);
        }
    }

    [HttpPost("{id}")]
    [Authorize(Policy = AuthPolicies.ELEVATED_ACCESS)]
    public async Task RevokeUserSession(Guid id, CancellationToken cancellationToken)
    {
        if (id == User.GetSessionId())
            throw new BadRequestException(Localizer[nameof(AppStrings.UserCantRemoveItsCurrentSessionsErrorMessage)]);

        var entityToDelete = await DbContext.UserSessions.FindAsync([id], cancellationToken)
            ?? throw new ResourceNotFoundException();

        DbContext.Remove(entityToDelete);

        await DbContext.SaveChangesAsync(cancellationToken);

        if (entityToDelete.SignalRConnectionId is not null)
        {
            await RevokeSession(entityToDelete.SignalRConnectionId, cancellationToken);
        }
    }

    [HttpPost("{userId}")]
    [Authorize(Policy = AuthPolicies.ELEVATED_ACCESS)]
    public async Task RevokeAllUserSessions(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == User.GetUserId())
            throw new BadRequestException(Localizer[nameof(AppStrings.UserCantRemoveAllItsSessionsErrorMessage)]);
        
        var userSessionConnectionIds = await DbContext.UserSessions.Where(us => us.UserId == userId && us.SignalRConnectionId != null)
                                                                   .Select(us => us.SignalRConnectionId!)
                                                                   .ToListAsync(cancellationToken);
        
        await DbContext.UserSessions.Where(us => us.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        foreach (var id in userSessionConnectionIds)
        {
            await RevokeSession(id, cancellationToken);
        }
    }


    private async Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
                    ?? throw new ResourceNotFoundException();

        return user;
    }

    private async Task RevokeSession(string connectionId, CancellationToken cancellationToken)
    {
        // Check out AppHub's comments for more info.
        await appHubContext.Clients.Client(connectionId)
                                   .SendAsync(SignalREvents.PUBLISH_MESSAGE, SharedPubSubMessages.SESSION_REVOKED, null, cancellationToken);
    }
}
