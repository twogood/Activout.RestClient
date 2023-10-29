using System;
using System.Net.Http;

namespace Activout.RestClient;

public interface IRequestLogger
{
    IDisposable TimeOperation(HttpRequestMessage httpRequestMessage);
}