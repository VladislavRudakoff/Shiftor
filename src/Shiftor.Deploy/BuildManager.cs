namespace Shiftor.Deploy;

public class BuildManager(DeployConfig config, ILogger<BuildManager> logger)
{
    /// <summary>
    /// Собирает проект с заданными параметрами.
    /// </summary>
    public async Task<string> Build()
    {
        string framework = config.TargetFramework.ToString().ToLower().Replace("_", ".");

        string args = $"publish {config.ProjectPath} -c {Constants.Build.ReleaseConfig} -f {framework}";

        if (config.DeploymentMode == DeploymentMode.SelfContained)
        {
            args += $" -r {config.RuntimeIdentifier}";
        }

        if (config.UseAot)
        {
            args += " --self-contained true -p:PublishAot=true";
        }

        ProcessStartInfo psi = new("dotnet", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process? process = Process.Start(psi);

        if (process is null)
        {
            throw new InvalidOperationException("Не удалось запустить dotnet");
        }

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode is not 0)
        {
            logger.LogError("Ошибка сборки проекта: {Error}", error);

            throw new InvalidOperationException($"Ошибка сборки: {error}");
        }

        logger.LogInformation("Сборка завершена: {Output}", output);

        return Path.Combine(Path.GetDirectoryName(config.ProjectPath)!, "bin", Constants.Build.ReleaseConfig, framework, config.RuntimeIdentifier ?? "publish");
    }
}
