using System.Text.Json;
using NBomber.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;
using NBomber.Plugins.Network.Ping;

namespace Demo.HTTP;

public class HttpClientArgsExample
{
    public void Run()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("http_scenario", async context =>
        {
            var request =
                Http.CreateRequest("GET", "https://nbomber.com")
                    .WithHeader("Content-Type", "application/json");
                  //.WithBody(new StringContent("{ some JSON }", Encoding.UTF8, "application/json"));

            var clientArgs = HttpClientArgs.Create(
                CancellationToken.None,
                httpCompletion: HttpCompletionOption.ResponseHeadersRead, // HttpCompletionOption: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpcompletionoption?view=net-7.0
                jsonOptions: JsonSerializerOptions.Default
            );

            var response = await Http.Send(httpClient, clientArgs, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithWorkerPlugins(
                new PingPlugin(PingPluginConfig.CreateDefault("nbomber.com")),
                new HttpMetricsPlugin(new [] { HttpVersion.Version1 })
            )
            .Run();
    }
}
