namespace Shiftor.Core;

public class DeployConfig
{
    public string ProjectPath { get; set; } = string.Empty;

    public TargetFramework TargetFramework { get; set; } = TargetFramework.Net8_0;

    public DeploymentMode DeploymentMode { get; set; } = DeploymentMode.FrameworkDependent;

    public string? RuntimeIdentifier { get; set; }

    public bool UseAot { get; set; }

    public ServerConfig Server { get; set; } = new();

    public class ServerConfig
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 22;

        public string Username { get; set; } = string.Empty;

        public string? EncryptedPassword { get; set; }

        public string ServiceName { get; set; } = string.Empty;
    }
}
