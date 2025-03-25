namespace Shiftor.Deploy;

public class SshManager(DeployConfig.ServerConfig serverConfig, ILogger<SshManager> logger)
{
    public async Task Deploy(string localPath)
    {
        using SshClient ssh = new(serverConfig.Host, serverConfig.Port, serverConfig.Username, DecryptPassword());

        ssh.Connect();

        await StopServiceAsync(ssh);
        await BackupCurrentVersionAsync(ssh);
        await CleanCurrentFilesAsync(ssh);
        await UploadNewFilesAsync(ssh, localPath);
        await SetPermissionsAsync(ssh);
        await StartServiceAsync(ssh);

        if (!await CheckServiceStatusAsync(ssh))
        {
            await RollbackAsync(ssh);
            throw new InvalidOperationException("Не удалось запустить сервис.");
        }

        ssh.Disconnect();
        logger.LogInformation(Constants.Messages.DeploySuccess);
    }

    private string DecryptPassword()
    {
        if (string.IsNullOrEmpty(serverConfig.EncryptedPassword))
        {
            return string.Empty;
        }

        // Простая обфускация через XOR вместо ProtectedData (кроссплатформенность)
        byte[] encryptedBytes = Convert.FromBase64String(serverConfig.EncryptedPassword);

        byte[] key = "shiftor_key"u8.ToArray(); // В продакшене нужен безопасный ключ

        byte[] decryptedBytes = new byte[encryptedBytes.Length];

        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            decryptedBytes[i] = (byte)(encryptedBytes[i] ^ key[i % key.Length]);
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public string EncryptPassword(string password)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(password);

        byte[] key = "shiftor_key"u8.ToArray();

        byte[] encryptedBytes = new byte[plainBytes.Length];

        for (int i = 0; i < plainBytes.Length; i++)
        {
            encryptedBytes[i] = (byte)(plainBytes[i] ^ key[i % key.Length]);
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    private async Task StopServiceAsync(SshClient ssh)
    {
        SshCommand command = ssh.RunCommand($"systemctl stop {serverConfig.ServiceName}");

        if (command.ExitStatus != 0)
        {
            logger.LogError("Ошибка остановки сервиса: {Error}", command.Error);
        }
        else
        {
            logger.LogInformation(Constants.Messages.ServiceStopped);
        }

        await Task.CompletedTask;
    }

    private async Task BackupCurrentVersionAsync(SshClient ssh)
    {
        string backupFile = $"{Constants.Server.BackupDirectory}/{Constants.Server.BackupPrefix}{DateTime.Now:yyyyMMdd_HHmmss}.tar.gz";

        SshCommand command = ssh.RunCommand($"mkdir -p {Constants.Server.BackupDirectory} && tar -czf {backupFile} -C {Constants.Server.AppDirectory} .");

        if (command.ExitStatus is not 0)
        {
            logger.LogError("Ошибка создания бекапа: {Error}", command.Error);
        }
        else
        {
            logger.LogInformation(Constants.Messages.BackupCreated, backupFile);
        }

        await Task.CompletedTask;
    }

    private async Task CleanCurrentFilesAsync(SshClient ssh)
    {
        SshCommand command = ssh.RunCommand($"find {Constants.Server.AppDirectory} -type f ! -name 'appsettings*.json' -delete");

        if (command.ExitStatus != 0)
        {
            logger.LogError("Ошибка очистки файлов: {Error}", command.Error);
        }
        else
        {
            logger.LogInformation("Текущие файлы очищены.");
        }

        await Task.CompletedTask;
    }

    private async Task UploadNewFilesAsync(SshClient ssh, string localPath)
    {
        using SftpClient sftp = new(serverConfig.Host, serverConfig.Port, serverConfig.Username, DecryptPassword());

        sftp.Connect();

        foreach (string file in Directory.GetFiles(localPath, "*"))
        {
            string remotePath = $"{Constants.Server.AppDirectory}/{Path.GetFileName(file)}";

            await using FileStream stream = File.OpenRead(file);

            sftp.UploadFile(stream, remotePath);
        }

        sftp.Disconnect();

        logger.LogInformation("Новые файлы загружены");

        await Task.CompletedTask;
    }

    private async Task SetPermissionsAsync(SshClient ssh)
    {
        SshCommand command = ssh.RunCommand($"chmod +x {Constants.Server.AppDirectory}/{serverConfig.ServiceName}");

        if (command.ExitStatus != 0)
        {
            logger.LogError("Ошибка установки прав: {Error}", command.Error);
        }
        else
        {
            logger.LogInformation("Права на запуск установлены.");
        }

        await Task.CompletedTask;
    }

    private async Task StartServiceAsync(SshClient ssh)
    {
        SshCommand command = ssh.RunCommand($"systemctl start {serverConfig.ServiceName}");

        if (command.ExitStatus != 0)
        {
            logger.LogError("Ошибка запуска сервиса: {Error}", command.Error);
        }
        else
        {
            logger.LogInformation(Constants.Messages.ServiceStarted);
        }

        await Task.CompletedTask;
    }

    private async Task<bool> CheckServiceStatusAsync(SshClient ssh)
    {
        SshCommand result = ssh.RunCommand($"systemctl is-active {serverConfig.ServiceName}");

        if (result.ExitStatus != 0)
        {
            logger.LogError("Ошибка проверки статуса сервиса: {Error}", result.Error);
            return false;
        }

        logger.LogInformation("Сервис успешно работает.");
        return true;
    }

    private async Task RollbackAsync(SshClient ssh)
    {
        SshCommand stopCommand = ssh.RunCommand($"systemctl stop {serverConfig.ServiceName}");

        if (stopCommand.ExitStatus != 0)
        {
            logger.LogError("Ошибка остановки сервиса при откате: {Error}", stopCommand.Error);
        }

        SshCommand cleanCommand = ssh.RunCommand($"rm -rf {Constants.Server.AppDirectory}/*");

        if (cleanCommand.ExitStatus != 0)
        {
            logger.LogError("Ошибка очистки при откате: {Error}", cleanCommand.Error);
        }

        SshCommand restoreCommand = ssh.RunCommand($"tar -xzf $(ls -t {Constants.Server.BackupDirectory}/{Constants.Server.BackupPrefix}*.tar.gz | head -1) -C {Constants.Server.AppDirectory}");

        if (restoreCommand.ExitStatus != 0)
        {
            logger.LogError("Ошибка восстановления бекапа: {Error}", restoreCommand.Error);
        }
        else
        {
            logger.LogWarning("Произведен откат к последнему бекапу");
        }

        await Task.CompletedTask;
    }
}
