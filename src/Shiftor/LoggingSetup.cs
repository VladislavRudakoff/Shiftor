namespace Shiftor;

public static class LoggingSetup
{
    public static void Configure(ILoggingBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(Constants.Paths.LogsFile, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.AddSerilog(dispose: true);
    }
}
