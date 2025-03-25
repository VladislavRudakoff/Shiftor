namespace Shiftor.Alerts;

/// <summary>
/// Управляет отправкой уведомлений через Telegram и Email.
/// </summary>
public class AlertManager
{
    private readonly ILogger<AlertManager> logger;
    private readonly AlertSettings settings;
    private readonly HttpClient httpClient;

    public AlertManager(ILogger<AlertManager> logger, ConfigProvider configProvider)
    {
        this.logger = logger;
        this.settings = configProvider.GetAlertSettings();
        this.httpClient = new HttpClient();
    }

    /// <summary>
    /// Асинхронно отправляет уведомление через Telegram и Email.
    /// </summary>
    /// <param name="message">Текст сообщения.</param>
    /// <param name="isError">Указывает, является ли сообщение ошибкой.</param>
    public async Task SendAlertAsync(string message, bool isError = false) =>
        await Task.WhenAll(SendTelegramAlertAsync(message, isError), SendEmailAlertAsync(message, isError));

    private async Task SendTelegramAlertAsync(string message, bool isError)
    {
        string prefix = isError ? "[ERROR] " : "[INFO] ";

        string url = $"https://api.telegram.org/bot{settings.TelegramToken}/sendMessage?chat_id={settings.TelegramChatId}&text={Uri.EscapeDataString(prefix + message)}";

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Не удалось отправить Telegram-уведомление: {Status}", response.StatusCode);
            }
            else
            {
                logger.LogInformation("Telegram-уведомление отправлено: {Message}", message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка отправки Telegram-уведомления: {Message}", message);
        }
    }

    private async Task SendEmailAlertAsync(string message, bool isError)
    {
        try
        {
            using SmtpClient client = new(settings.SmtpServer)
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(settings.SmtpUser, settings.SmtpPassword),
                EnableSsl = true
            };

            MailMessage mail = new(settings.SmtpUser, settings.EmailTo, isError ? "[ERROR] Deploy Alert" : "[INFO] Deploy Alert", message);

            await client.SendMailAsync(mail);

            logger.LogInformation("Email-уведомление отправлено: {Message}", message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось отправить Email-уведомление: {Message}", message);
        }
    }
}
