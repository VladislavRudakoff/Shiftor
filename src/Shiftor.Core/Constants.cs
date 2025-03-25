namespace Shiftor.Core;

public class Constants
{
    public static class Options
    {
        public const string Yes = "Да";

        public const string No = "Нет";

        public static readonly string[] YesNo = [Yes, No];
    }

    public static class Build
    {
        public const string ReleaseConfig = "Release";

        public const string DefaultRuntimeLinux = "linux-x64";

        public const string DefaultRuntimeWindows = "win-x64";

        public static readonly string[] Runtimes = [DefaultRuntimeLinux, DefaultRuntimeWindows];
    }

    public static class Paths
    {
        public const string ConfigFile = "deploy_config.json";
        public const string ServersFile = "servers.json";
        public const string LogsFile = "deploy_logs.txt";
    }

    public static class Server
    {
        public const int DefaultPort = 22;

        public const string AppDirectory = "~/app";

        public const string BackupDirectory = "~/backups";

        public const string BackupPrefix = "backup_";
    }

    public static class Messages
    {
        public const string DeploySuccess = "Деплой успешно завершен";

        public const string DeployFailed = "Ошибка деплоя: {0}";

        public const string ServiceStopped = "Сервис остановлен";

        public const string ServiceStarted = "Сервис запущен";

        public const string BackupCreated = "Бекап создан: {0}";
    }

    public static class Alerts
    {
        public const string ErrorPrefix = "[ERROR] ";

        public const string InfoPrefix = "[INFO] ";

        public const string TelegramBaseUrl = "https://api.telegram.org/bot";

        public const int SmtpPort = 587;
    }
}
