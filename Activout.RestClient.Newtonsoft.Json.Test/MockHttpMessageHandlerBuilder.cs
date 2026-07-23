using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Extensions.Http;
using RichardSzalay.MockHttp;

namespace Activout.RestClient.Newtonsoft.Json.Test;

internal sealed class MockHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
{
    private readonly MockHttpMessageHandler _mockHttp;

    public MockHttpMessageHandlerBuilder(MockHttpMessageHandler mockHttp)
    {
        _mockHttp = mockHttp;
    }

    [DisallowNull]
    public override string? Name { get; set; }

    public override HttpMessageHandler PrimaryHandler
    {
        get => _mockHttp;
        set { }
    }

    public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

    public override HttpMessageHandler Build()
    {
        return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
    }
}