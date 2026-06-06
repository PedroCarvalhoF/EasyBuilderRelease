using System.Text;

namespace EasyBuilderRelease;

internal enum ReleaseKind
{
    MauiAndroid,
    MauiWindows,
    Api,
    PrintServer
}

internal static class ReleaseKindExtensions
{
    public static string DisplayName(this ReleaseKind kind) => kind switch
    {
        ReleaseKind.MauiAndroid => "MAUI Android",
        ReleaseKind.MauiWindows => "MAUI Windows",
        ReleaseKind.Api => "API",
        ReleaseKind.PrintServer => "Servidor Impressao",
        _ => kind.ToString()
    };

    public static string OutputFolderName(this ReleaseKind kind) => kind switch
    {
        ReleaseKind.MauiAndroid => "01_MauiAndroid",
        ReleaseKind.MauiWindows => "02_MauiWindows",
        ReleaseKind.Api => "03_API",
        ReleaseKind.PrintServer => "04_ServidorImpressao",
        _ => kind.ToString()
    };

    public static string LogFileName(this ReleaseKind kind) => kind switch
    {
        ReleaseKind.MauiAndroid => "maui-android.log",
        ReleaseKind.MauiWindows => "maui-windows.log",
        ReleaseKind.Api => "api.log",
        ReleaseKind.PrintServer => "servidor-impressao.log",
        _ => $"{kind}.log"
    };

    public static string TargetFramework(this ReleaseKind kind) => kind switch
    {
        ReleaseKind.MauiAndroid => "net9.0-android",
        ReleaseKind.MauiWindows => "net9.0-windows10.0.19041.0",
        ReleaseKind.Api => "net9.0",
        ReleaseKind.PrintServer => "net9.0-windows",
        _ => ""
    };

    public static IReadOnlyList<string> BuildDotnetArguments(
        this ReleaseKind kind,
        string projectFile,
        string outputDirectory)
    {
        var args = new List<string>
        {
            "publish",
            projectFile,
            "-c",
            "Release",
            "-f",
            kind.TargetFramework()
        };

        switch (kind)
        {
            case ReleaseKind.MauiAndroid:
                args.Add("-p:AndroidPackageFormat=apk");
                break;
            case ReleaseKind.MauiWindows:
                args.Add("-p:WindowsPackageType=None");
                args.Add("-p:RuntimeIdentifierOverride=win10-x64");
                break;
            case ReleaseKind.PrintServer:
                args.Add("--runtime");
                args.Add("win-x64");
                args.Add("--self-contained");
                args.Add("true");
                break;
        }

        args.Add("-o");
        args.Add(outputDirectory);
        args.Add("--nologo");
        return args;
    }

    public static string ToCommandText(this ReleaseKind kind, string projectFile, string outputDirectory)
    {
        var builder = new StringBuilder("dotnet");
        foreach (var argument in kind.BuildDotnetArguments(projectFile, outputDirectory))
        {
            builder.Append(' ');
            builder.Append(Quote(argument));
        }

        return builder.ToString();
    }

    private static string Quote(string value)
    {
        if (value.Length == 0)
        {
            return "\"\"";
        }

        return value.Any(char.IsWhiteSpace)
            ? $"\"{value.Replace("\"", "\\\"")}\""
            : value;
    }
}
