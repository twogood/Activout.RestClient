// Performance check, not part of `dotnet test`.
// Run: dotnet run -c Release --project Activout.RestClient.Perf <scenario name filter>
// One scenario per process: order skews results.
using System.Diagnostics;
using Activout.RestClient;
using RichardSzalay.MockHttp;

var mockHttp = new MockHttpMessageHandler();
mockHttp.When("*").Respond("text/plain", "ok");
var httpClient = mockHttp.ToHttpClient();
var factory = new RestClientFactory();
var scenarios = new List<(string Name, int N, Action Action)>();

IPerfClient Build() => factory.CreateBuilder()
    .With(httpClient)
    .BaseUri(new Uri("https://example.com/api"))
    .Build<IPerfClient>();

Add("new client per call (GET)", 20_000, () => Build().Get(1, "q").GetAwaiter().GetResult());
Add("new client per call (POST)", 20_000, () => Build().Post(1, "body").GetAwaiter().GetResult());
T BuildMany<T>() where T : class => factory.CreateBuilder()
    .With(httpClient)
    .BaseUri(new Uri("https://example.com/api"))
    .Build<T>();

Add("new client, 5 methods", 20_000, () =>
{
    var c = BuildMany<IFiveMethods>();
    c.M0(1, "q").GetAwaiter().GetResult();
    c.M1(1, "q").GetAwaiter().GetResult();
    c.M2(1, "q").GetAwaiter().GetResult();
    c.M3(1, "q").GetAwaiter().GetResult();
    c.M4(1, "q").GetAwaiter().GetResult();
});
Add("new client, 10 methods", 20_000, () =>
{
    var c = BuildMany<ITenMethods>();
    c.M0(1, "q").GetAwaiter().GetResult();
    c.M1(1, "q").GetAwaiter().GetResult();
    c.M2(1, "q").GetAwaiter().GetResult();
    c.M3(1, "q").GetAwaiter().GetResult();
    c.M4(1, "q").GetAwaiter().GetResult();
    c.M5(1, "q").GetAwaiter().GetResult();
    c.M6(1, "q").GetAwaiter().GetResult();
    c.M7(1, "q").GetAwaiter().GetResult();
    c.M8(1, "q").GetAwaiter().GetResult();
    c.M9(1, "q").GetAwaiter().GetResult();
});
var client = Build();
Add("same client (GET)", 200_000, () => client.Get(1, "q").GetAwaiter().GetResult());

var matches = scenarios.FindAll(s => args.Length > 0 && s.Name.Contains(args[0]));
if (matches.Count != 1)
{
    Console.Error.WriteLine($"Give a filter matching exactly one scenario (matched {matches.Count}):");
    scenarios.ForEach(s => Console.Error.WriteLine($"  {s.Name}"));
    return 1;
}
Measure(matches[0].Name, matches[0].N, matches[0].Action);
return 0;

void Add(string name, int n, Action action) => scenarios.Add((name, n, action));

void Measure(string name, int n, Action action)
{
    for (var i = 0; i < n / 10; i++) action(); // warm up (JIT tiering) before timing
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

[Path("items")]
[Header("X-Perf", "1")]
public interface IFiveMethods
{
    [Get("/m0/{id}")]
    Task<string> M0([PathParam] int id, [QueryParam] string q);

    [Get("/m1/{id}")]
    Task<string> M1([PathParam] int id, [QueryParam] string q);

    [Get("/m2/{id}")]
    Task<string> M2([PathParam] int id, [QueryParam] string q);

    [Get("/m3/{id}")]
    Task<string> M3([PathParam] int id, [QueryParam] string q);

    [Get("/m4/{id}")]
    Task<string> M4([PathParam] int id, [QueryParam] string q);
}

[Path("items")]
[Header("X-Perf", "1")]
public interface ITenMethods
{
    [Get("/m0/{id}")]
    Task<string> M0([PathParam] int id, [QueryParam] string q);

    [Get("/m1/{id}")]
    Task<string> M1([PathParam] int id, [QueryParam] string q);

    [Get("/m2/{id}")]
    Task<string> M2([PathParam] int id, [QueryParam] string q);

    [Get("/m3/{id}")]
    Task<string> M3([PathParam] int id, [QueryParam] string q);

    [Get("/m4/{id}")]
    Task<string> M4([PathParam] int id, [QueryParam] string q);

    [Get("/m5/{id}")]
    Task<string> M5([PathParam] int id, [QueryParam] string q);

    [Get("/m6/{id}")]
    Task<string> M6([PathParam] int id, [QueryParam] string q);

    [Get("/m7/{id}")]
    Task<string> M7([PathParam] int id, [QueryParam] string q);

    [Get("/m8/{id}")]
    Task<string> M8([PathParam] int id, [QueryParam] string q);

    [Get("/m9/{id}")]
    Task<string> M9([PathParam] int id, [QueryParam] string q);
}
