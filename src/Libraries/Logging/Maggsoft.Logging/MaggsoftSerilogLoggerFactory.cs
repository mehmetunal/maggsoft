using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Maggsoft.Logging.Logging;

public class MaggsoftSerilogLoggerFactory
{
    public static ILogger CreateSerilogLogger()
    {
        return new LoggerConfiguration()
            .WriteTo.Seq("http://localhost:5341")
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.WithProperty("AppName", "Serilog Sample")
            .Enrich.WithProperty("Environment", "development")
            .Enrich.With(new ThreadIdEnricher()).CreateLogger();
    }

    public static ILogger CreateSerilogLogger(IConfiguration configuration)
    {
        string applicationName = configuration["Serilog:ApplicationName"];
        
        return new LoggerConfiguration()
            .Enrich.WithProperty("ApplicationContext", applicationName)
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}