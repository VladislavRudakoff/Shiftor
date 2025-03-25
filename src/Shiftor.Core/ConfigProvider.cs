namespace Shiftor.Core;

public class ConfigProvider
{
    private readonly IConfiguration configuration;

    public ConfigProvider()
    {
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    /// <summary>
    /// Получает конфигурацию деплоя из файла.
    /// </summary>
    public DeployConfig LoadDeployConfig()
    {
        if (!File.Exists(Constants.Paths.ConfigFile))
        {
            return new DeployConfig();
        }

        string json = File.ReadAllText(Constants.Paths.ConfigFile);
        return JsonSerializer.Deserialize<DeployConfig>(json) ?? new DeployConfig();
    }

    /// <summary>
    /// Сохраняет конфигурацию деплоя в файл.
    /// </summary>
    public void SaveDeployConfig(DeployConfig config)
    {
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(Constants.Paths.ConfigFile, json);
    }

    /// <summary>
    /// Получает настройки алертов из appsettings.json.
    /// </summary>
    public AlertSettings GetAlertSettings()
    {
        AlertSettings settings = new();

        configuration.GetSection("AlertSettings").Bind(settings);

        return settings;
    }
}
