namespace RingBearer.Core.Models;

public class ExportOptions
{
    public string FilePath { get; set; } = string.Empty;
    public string? Password { get; set; }
    public ExportFormat Format { get; set; }
}
