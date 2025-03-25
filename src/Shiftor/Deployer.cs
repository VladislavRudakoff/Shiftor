namespace Shiftor;

public class Deployer
{
    private readonly ConsoleUI _ui;
    private readonly ILogger<Deployer> _logger;
    private readonly ServerMonitor _monitor;
    private readonly AlertManager _alerts;
    private DeployConfig _config = new();
    private const string ConfigFile = "deploy_config.json";

    public Deployer(ConsoleUI ui, ILogger<Deployer> logger)
    {
        _ui = ui;
        _logger = logger;
        _monitor = new ServerMonitor(logger);
        _alerts = new AlertManager(logger, "telegram_token", "chat_id", "smtp.gmail.com", "user@gmail.com", "password", "to@gmail.com"); // Настрой в реальном проекте
        LoadConfig();
    }

    public void Run()
    {
        if (!TryUseExistingConfig())
        {
            ConfigureBuild();
            ConfigureServer();
            SaveConfig();
        }
        ExecuteDeploy();
    }

    private bool TryUseExistingConfig()
    {
        if (string.IsNullOrEmpty(_config.ProjectPath)) return false;
        Console.WriteLine("Найдены прошлые настройки:");
        Console.WriteLine(JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
        return _ui.SelectOption("Использовать их?", ["Да", "Нет"]) == "Да";
    }

    private void ConfigureBuild()
    {
        _config.ProjectPath = _ui.ReadString("Путь к .csproj файлу");
        _config.TargetFramework = Enum.Parse<TargetFramework>(_ui.SelectOption("Выберите Target Framework", ["Net8_0", "Net9_0"]));
        _config.DeploymentMode = Enum.Parse<DeploymentMode>(_ui.SelectOption("Режим деплоя", ["SelfContained", "FrameworkDependent"]));
        if (_config.DeploymentMode == DeploymentMode.SelfContained)
            _config.RuntimeIdentifier = _ui.SelectOption("Runtime Identifier", ["linux-x64", "win-x64"]);
        _config.UseAot = _ui.SelectOption("Использовать AOT?", ["Да", "Нет"]) == "Да";
    }

    private void ConfigureServer()
    {
        _config.Server.Host = _ui.ReadString("Хост сервера");
        _config.Server.Port = int.Parse(_ui.ReadString("Порт сервера (по умолчанию 22)"));
        _config.Server.Username = _ui.ReadString("Имя пользователя");
        string password = _ui.ReadString("Пароль");
        _config.Server.EncryptedPassword = EncryptPassword(password);
        _config.Server.ServiceName = _ui.ReadString("Имя сервиса (например, myapp.service)");
    }

    private string EncryptPassword(string password)
    {
        byte[] encrypted = ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    private void ExecuteDeploy()
    {
        BuildManager builder = new(_config);
        SshManager ssh = new(_config.Server, _logger);
        string outputPath = builder.Build();
        try
        {
            ssh.Deploy(outputPath);
            _monitor.AddServer(_config, DateTime.Now);
            _alerts.SendAlertAsync("Деплой прошел успешно!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка деплоя.");
            _alerts.SendAlertAsync($"Ошибка деплоя: {ex.Message}", true);
        }
    }

    private void SaveConfig() => File.WriteAllText(ConfigFile, JsonSerializer.Serialize(_config));
    private void LoadConfig()
    {
        if (File.Exists(ConfigFile))
            _config = JsonSerializer.Deserialize<DeployConfig>(File.ReadAllText(ConfigFile)) ?? new();
    }
}
