IServiceProvider services = ConfigureServices();
Deployer deployer = services.GetRequiredService<Deployer>();
deployer.Run();

return;

static IServiceProvider ConfigureServices() =>
    new ServiceCollection()
        .AddLogging(LoggingSetup.Configure)
        .AddSingleton<ConsoleUI>()
        .AddSingleton<Deployer>()
        .AddSingleton<BuildManager>()
        .AddSingleton<SshManager>()
        .AddSingleton<AlertManager>()
        .AddSingleton<ServerMonitor>()
        .BuildServiceProvider();
