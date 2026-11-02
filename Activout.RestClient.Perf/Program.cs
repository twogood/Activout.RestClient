// Performance check, not part of `dotnet test`. Run: dotnet run -c Release --project Activout.RestClient.Perf
using System.Diagnostics;
using Activout.RestClient;
using RichardSzalay.MockHttp;

var mockHttp = new MockHttpMessageHandler();
mockHttp.When("*").Respond("text/plain", "ok");
var httpClient = mockHttp.ToHttpClient();
var factory = new RestClientFactory();

IPerfClient Build() => factory.CreateBuilder()
    .With(httpClient)
    .BaseUri(new Uri("https://example.com/api"))
    .Build<IPerfClient>();

Measure("warmup", 2_000, () => Build().Get(1, "q").GetAwaiter().GetResult());
Measure("new client per call (GET)", 20_000, () => Build().Get(1, "q").GetAwaiter().GetResult());
Measure("new client per call (POST)", 20_000, () => Build().Post(1, "body").GetAwaiter().GetResult());
var client = Build();
Measure("same client (GET)", 200_000, () => client.Get(1, "q").GetAwaiter().GetResult());

static void Measure(string name, int n, Action action)
{
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < n; i++) action();
    sw.Stop();
    Console.WriteLine($"{name,-30} {n,8} ops {sw.ElapsedMilliseconds,7} ms {sw.Elapsed.TotalMicroseconds / n,8:F2} µs/op");
}

[Path("items")]
[Header("X-Perf", "1")]
public interface IPerfClient
{
    [Get("/{id}")]
    Task<string> Get([PathParam] int id, [QueryParam] string q);

    [Post("/{id}")]
    Task<string> Post([PathParam] int id, string body);
}
