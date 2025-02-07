namespace Bit.TemplatePlayground.Client.Core.Services;

public partial class NoOpLocalHttpServer : ILocalHttpServer
{
    public int Start(CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <summary>
    /// <inheritdoc cref="ILocalHttpServer.ShouldUseForSocialSignIn"/>
    /// </summary>
    public bool ShouldUseForSocialSignIn() => false;
}
