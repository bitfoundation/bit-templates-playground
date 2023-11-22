using Bit.TemplatePlayground.Iac;
using Pulumi;

public class Program
{
    static Task<int> Main() => Deployment.RunAsync<BpStack>();
}
