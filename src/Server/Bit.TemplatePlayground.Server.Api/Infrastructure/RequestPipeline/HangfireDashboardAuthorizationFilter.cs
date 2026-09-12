using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Bit.TemplatePlayground.Server.Api.Infrastructure.RequestPipeline;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        return context.GetHttpContext().User.HasClaim(AppClaimTypes.FEATURES, AppFeatures.System.Jobs_Manage);
    }
}
