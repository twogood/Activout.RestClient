using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Activout.RestClient.Newtonsoft.Json.Test
{
    public static class LoggerFactoryHelpers
    {
        public static ILoggerFactory CreateLoggerFactory(ITestOutputHelper outputHelper)
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Activout.RestClient", LogLevel.Debug)
                    .AddXUnit(outputHelper);
            });
        }
    }
}