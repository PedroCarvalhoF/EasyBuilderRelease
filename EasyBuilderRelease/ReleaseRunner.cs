using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace EasyBuilderRelease;

internal sealed class ReleaseRunner
{
    public ReleaseValidationResult ValidateProject(ReleaseKind kind, string projectFile)
    {
        if (string.IsNullOrWhiteSpace(projectFile))
        {
            return ReleaseValidationResult.Fail("Selecione um arquivo .csproj.");
        }

        if (!File.Exists(projectFile))
        {
            return ReleaseValidationResult.Fail($"Projeto nao encontrado: {projectFile}");
        }

        XDocument document;
        try
        {
            document = XDocument.Load(projectFile);
        }
        catch (Exception ex)
        {
            return ReleaseValidationResult.Fail($"Nao foi possivel ler o .csproj: {ex.Message}");
        }

        var sdk = document.Root?.Attribute("Sdk")?.Value ?? "";
        var values = document
            .Descendants()
            .Where(element => element.Name.LocalName is "TargetFramework" or "TargetFrameworks" or "UseMaui" or "UseWindowsForms")
            .GroupBy(element => element.Name.LocalName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => string.Join(";", group.Select(element => element.Value)),
                StringComparer.OrdinalIgnoreCase);

        var targetText = string.Join(";", values
            .Where(pair => pair.Key is "TargetFramework" or "TargetFrameworks")
            .Select(pair => pair.Value));

        var useMaui = values.TryGetValue("UseMaui", out var useMauiValue)
            && string.Equals(useMauiValue.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        var useWindowsForms = values.TryGetValue("UseWindowsForms", out var useWindowsFormsValue)
            && string.Equals(useWindowsFormsValue.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        if ((kind is ReleaseKind.MauiAndroid or ReleaseKind.MauiWindows) && !useMaui)
        {
            return ReleaseValidationResult.Fail($"{kind.DisplayName()} exige <UseMaui>true</UseMaui>.");
        }

        if ((kind is ReleaseKind.Api or ReleaseKind.PrintServer or ReleaseKind.BlazorWeb)
            && !sdk.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase))
        {
            return ReleaseValidationResult.Fail($"{kind.DisplayName()} exige Project Sdk=\"Microsoft.NET.Sdk.Web\".");
        }

        if (kind is ReleaseKind.WindowsForms && !useWindowsForms)
        {
            return ReleaseValidationResult.Fail($"{kind.DisplayName()} exige <UseWindowsForms>true</UseWindowsForms>.");
        }

        var expectedFramework = kind.TargetFramework();
        if (!targetText.Contains(expectedFramework, StringComparison.OrdinalIgnoreCase))
        {
            return ReleaseValidationResult.Fail($"{kind.DisplayName()} exige target {expectedFramework}.");
        }

        return ReleaseValidationResult.Success();
    }

    public async Task<ReleaseRunResult> RunAsync(
        ReleaseItem item,
        string logsDirectory,
        Action<string> writeItemLog,
        Action<string> writeSummaryLog,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(item.OutputDirectory);
        Directory.CreateDirectory(logsDirectory);

        var logFile = Path.Combine(logsDirectory, item.Kind.LogFileName());
        var commandText = item.CommandText;

        await using var writer = new StreamWriter(logFile, false, new UTF8Encoding(false));
        var writerLock = new object();

        void WriteLine(string text)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {text}";

            lock (writerLock)
            {
                writer.WriteLine(line);
                writer.Flush();
            }

            writeItemLog(line);
        }

        WriteLine(commandText);
        writeSummaryLog($"{item.Kind.DisplayName()}: iniciado.");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(item.ProjectFile) ?? Environment.CurrentDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in item.Kind.BuildDotnetArguments(item.ProjectFile, item.OutputDirectory))
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    WriteLine(args.Data);
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    WriteLine($"ERR: {args.Data}");
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Nao foi possivel iniciar o processo dotnet.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
            process.WaitForExit();

            item.ExitCode = process.ExitCode;

            if (process.ExitCode == 0 && item.Kind == ReleaseKind.MauiAndroid)
            {
                NormalizeAndroidOutput(item.OutputDirectory, writeItemLog, writeSummaryLog);
            }

            if (process.ExitCode == 0 && item.Kind == ReleaseKind.PrintServer)
            {
                CopyPrinterServiceScript(item.ProjectFile, item.OutputDirectory, writeItemLog);
            }

            var succeeded = process.ExitCode == 0;
            writeSummaryLog($"{item.Kind.DisplayName()}: {(succeeded ? "concluido" : "falhou")} (exit code {process.ExitCode}).");

            return new ReleaseRunResult
            {
                Item = item,
                LogFile = logFile,
                CommandText = commandText,
                ExitCode = process.ExitCode,
                Succeeded = succeeded
            };
        }
        catch (Exception ex)
        {
            item.ExitCode = -1;
            WriteLine($"ERR: {ex.Message}");
            writeSummaryLog($"{item.Kind.DisplayName()}: falhou ao iniciar/executar.");

            return new ReleaseRunResult
            {
                Item = item,
                LogFile = logFile,
                CommandText = commandText,
                ExitCode = -1,
                Succeeded = false
            };
        }
    }

    private static void NormalizeAndroidOutput(
        string outputDirectory,
        Action<string> writeItemLog,
        Action<string> writeSummaryLog)
    {
        var allFiles = Directory
            .EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .ToList();

        var signedApks = allFiles
            .Where(file => file.Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase)
                && file.Name.Contains("-Signed", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var keep = signedApks.Count > 0
            ? signedApks
            : allFiles
                .Where(file => file.Extension.Equals(".apk", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Take(1)
                .ToList();

        if (keep.Count == 0)
        {
            writeSummaryLog("MAUI Android: nenhum APK encontrado na saida.");
            return;
        }

        if (signedApks.Count == 0)
        {
            writeSummaryLog($"MAUI Android: APK assinado nao encontrado; mantendo {keep[0].Name}.");
        }

        var keepSet = keep.Select(file => file.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var file in allFiles.Where(file => !keepSet.Contains(file.FullName)))
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                writeItemLog($"[{DateTime.Now:HH:mm:ss}] Aviso: nao foi possivel remover {file.Name}: {ex.Message}");
            }
        }

        foreach (var directory in Directory
            .EnumerateDirectories(outputDirectory, "*", SearchOption.AllDirectories)
            .OrderByDescending(path => path.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
    }

    private static void CopyPrinterServiceScript(
        string projectFile,
        string outputDirectory,
        Action<string> writeItemLog)
    {
        var projectDirectory = Path.GetDirectoryName(projectFile);
        if (projectDirectory is null)
        {
            return;
        }

        var serviceScript = Path.Combine(projectDirectory, "servico-impressao.bat");
        if (!File.Exists(serviceScript))
        {
            return;
        }

        var destination = Path.Combine(outputDirectory, Path.GetFileName(serviceScript));
        File.Copy(serviceScript, destination, overwrite: true);
        writeItemLog($"[{DateTime.Now:HH:mm:ss}] Copiado {Path.GetFileName(serviceScript)} para a pasta de release.");
    }
}
