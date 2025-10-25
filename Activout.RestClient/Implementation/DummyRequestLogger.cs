using System;
using System.Net.Http;

namespace Activout.RestClient.Implementation;

internal sealed class DummyRequestLogger : IRequestLogger, IDisposable
{
    public static IRequestLogger Instance { get; } = new DummyRequestLogger();

    private DummyRequestLogger()
    {
    }

    public IDisposable TimeOperation(HttpRequestMessage httpRequestMessage) => this;

    public void Dispose()
    {
    }
}