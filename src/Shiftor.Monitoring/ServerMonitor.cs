namespace Shiftor.Monitoring;

public class ServerMonitor
{
    private const string ServersFile = "servers.json";

    private readonly ILogger<ServerMonitor> logger;
    private readonly List<ServerState> servers = [];

    public ServerMonitor(ILogger<ServerMonitor> logger)
    {
        this.logger = logger;
        LoadServers();
    }

    public void AddServer(DeployConfig config, DateTime deployDate)
    {
        servers.Add(new ServerState
        {
            Host = config.Server.Host,
            ServiceName = config.Server.ServiceName,
            LastDeployDate = deployDate,
            Status = "Unknown"
        });

        SaveServers();
    }

    public void UpdateStatus()
    {
        foreach (ServerState server in servers)
        {
            using SshClient ssh = new(server.Host, 22, "user", "password"); // Заменить на реальные данные из конфига
            ssh.Connect();
            SshCommand result = ssh.RunCommand($"systemctl is-active {server.ServiceName}");
            server.Status = result.ExitStatus == 0 ? "Active" : "Failed";
            ssh.Disconnect();
        }

        SaveServers();
    }

    public string GetLogs(string host, bool realTime = false)
    {
        ServerState? server = servers.Find(s => s.Host == host);

        if (server is null)
        {
            return "Сервер не найден.";
        }

        using SshClient ssh = new(server.Host, 22, "user", "password"); // Заменить на реальные данные

        ssh.Connect();

        string command = realTime ? $"tail -f /var/log/{server.ServiceName}.log" : $"cat /var/log/{server.ServiceName}.log";

        SshCommand result = ssh.RunCommand(command);

        ssh.Disconnect();

        return result.Result;
    }

    public void DisplayServers()
    {
        Console.Clear();

        foreach (ServerState server in servers)
        {
            Console.WriteLine($"Сервер: {server.Host}, Сервис: {server.ServiceName}, Статус: {server.Status}, Деплой: {server.LastDeployDate}");
        }
    }

    private void LoadServers()
    {
        if (File.Exists(ServersFile))
        {
            servers.AddRange(JsonSerializer.Deserialize<List<ServerState>>(File.ReadAllText(ServersFile)) ?? []);
        }
    }

    private void SaveServers() => File.WriteAllText(ServersFile, JsonSerializer.Serialize(servers, new JsonSerializerOptions { WriteIndented = true }));
}

public class ServerState
{
    public string Host { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastDeployDate { get; set; }
}
