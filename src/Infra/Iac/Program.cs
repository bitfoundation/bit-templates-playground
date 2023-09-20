using Pulumi;
using Bit.TemplatePlayground.Iac;

public class Program
{
    static Task<int> Main() => Deployment.RunAsync<AdStack>();
}
