using Microsoft.Extensions.Localization;
using RingBearer.Core.Constants;
using RingBearer.Core.Crypto;
using RingBearer.Core.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace RingBearer.Core.Storage;
public class FileManager(
    IEncryptionService encryptionService,
    IStringLocalizer localizer) : IFileManager
{
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly IStringLocalizer _localizer = localizer;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

    public static bool FileExist(string filePath = "")
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// Loads the entries from the file. If the file does not exist, it creates a new one.
    /// </summary>
    /// <param name="masterKey"> The master key used for decryption.</param>
    /// <returns> A list of entries.</returns>
    /// <param name="filePath"> The path to the file.</param>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the file cannot be loaded or decrypted.</exception>
    public async Task<List<EntryModel>> LoadAsync(string masterKey, string filePath  ="")
    {
        if (string.IsNullOrWhiteSpace(masterKey))
            throw new ArgumentException(_localizer["MasterKeyNullError"]);

        if (!File.Exists(filePath))
        {
            // Si el archivo no existe, lo creamos y devolvemos la lista vacía.
            await SaveAsync([], masterKey, filePath);
            return [];  // Retornamos una lista vacía
        }

        try
        {
            return await _encryptionService.LoadEntriesAsync(masterKey, filePath) ?? [];
        }
        catch (CryptographicException)
        {
            // Excepción específica para errores de desencriptación (contraseña incorrecta)
            throw new InvalidOperationException(_localizer["InvalidMasterKeyError"]);
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            throw new InvalidOperationException(_localizer["LoadFileError"], ex);
        }
    }

    /// <summary>
    /// Saves the entries to the file.
    /// </summary>
    /// <param name="entries"> The list of entries to save.</param>
    /// <param name="masterKey"> The master key used for encryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the file cannot be saved or encrypted.</exception>
    public async Task SaveAsync(List<EntryModel> entries, string masterKey, string filePath ="")
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (string.IsNullOrWhiteSpace(masterKey))
            throw new ArgumentException(_localizer["MasterKeyNullError"]);

        try
        {
            string json = JsonSerializer.Serialize(entries, _jsonSerializerOptions);
            byte[] encrypted = _encryptionService.Encrypt(json, masterKey);

            await File.WriteAllBytesAsync(filePath, encrypted);
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            throw new InvalidOperationException(_localizer["SaveFileError"], ex);
        }
    }
}
