namespace Shiftor.Core.Settings;

public class AlertSettings
{
    public string TelegramToken { get; set; } = string.Empty;
    public string TelegramChatId { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string EmailTo { get; set; } = string.Empty;
}
