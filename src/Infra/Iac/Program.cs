using Pulumi;
using Bit.AdminPanel.Iac;

public class Program
{
    static Task<int> Main() => Deployment.RunAsync<AdStack>();
}
