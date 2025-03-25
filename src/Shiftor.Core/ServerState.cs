namespace Shiftor.Core;

public class ServerState
{
    public string Host { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("LastDeployDate")]
    public DateTime LastDeployDate { get; set; }
}
