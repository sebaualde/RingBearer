using Microsoft.Extensions.Localization;
using RingBearer.Core.Crypto;
using RingBearer.Core.Models;
using System.Text.Json;

namespace RingBearer.Core.Services;

public class FileExchangeService(
    IEncryptionService encryptionService, 
    IStringLocalizer localizer) : IFileExchangeService
{
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly IStringLocalizer _localizer = localizer;

    // Cache the JsonSerializerOptions instance to avoid creating a new one for each operation  
    private static readonly JsonSerializerOptions _cachedJsonSerializerOptions = new() { WriteIndented = true };

    public async Task ExportAsync(IEnumerable<EntryModel> entries, ExportOptions options)
    {
        ValidateOptions(options);

        string json = JsonSerializer.Serialize(entries, _cachedJsonSerializerOptions);

        byte[] data = string.IsNullOrWhiteSpace(options.Password)
            ? System.Text.Encoding.UTF8.GetBytes(json)
            : _encryptionService.Encrypt(json, options.Password!);

        await File.WriteAllBytesAsync(options.FilePath, data);
    }

    public async Task<List<EntryModel>> ImportAsync(ExportOptions options)
    {
        ValidateOptions(options);

        try
        {
        byte[] data = await File.ReadAllBytesAsync(options.FilePath);

        string jsonFile = string.IsNullOrWhiteSpace(options.Password)
            ? System.Text.Encoding.UTF8.GetString(data)
            : await _encryptionService.DecryptAsync(options.Password!, options.FilePath);

            List<EntryModel>? entryModels = JsonSerializer.Deserialize<List<EntryModel>>(jsonFile);
            return entryModels ?? [];
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(_localizer["DeserializationFailedException"], ex);
        }
    }

    private void ValidateOptions(ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!Enum.IsDefined(options.Format))
            throw new NotSupportedException(_localizer["ExportFormatNotSupportedException"]);

        if (options.Format == ExportFormat.EncryptedBinary && string.IsNullOrWhiteSpace(options.Password))
            throw new ArgumentException(_localizer["PasswordRequiredForEncryptedExportException"]);
    }
}
