namespace EasyBuilderRelease;

internal sealed class ReleaseItem
{
    public ReleaseItem(ReleaseKind kind, string projectFile)
    {
        Kind = kind;
        ProjectFile = projectFile;
        OutputDirectory = kind.OutputFolderName();
        Status = "Pronto";
    }

    public ReleaseKind Kind { get; }

    public string ProjectFile { get; set; }

    public string OutputDirectory { get; set; }

    public string Status { get; set; }

    public string CommandText => Kind.ToCommandText(ProjectFile, OutputDirectory);

    public int? ExitCode { get; set; }
}

internal sealed class ReleaseRunResult
{
    public required ReleaseItem Item { get; init; }

    public required string LogFile { get; init; }

    public required string CommandText { get; init; }

    public required int ExitCode { get; init; }

    public required bool Succeeded { get; init; }
}

internal sealed class ReleaseValidationResult
{
    private ReleaseValidationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }

    public bool IsValid { get; }

    public string Message { get; }

    public static ReleaseValidationResult Success() => new(true, "");

    public static ReleaseValidationResult Fail(string message) => new(false, message);
}
