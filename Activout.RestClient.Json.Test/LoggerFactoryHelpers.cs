using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Activout.RestClient.Json.Test;

public static class LoggerFactoryHelpers
{
    public static ILoggerFactory CreateLoggerFactory(ITestOutputHelper outputHelper)
    {
        return LoggerFactory.Create(builder =>
        {
            builder.AddXUnit(outputHelper);
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}