using RingBearer.Core.Models;

namespace RingBearer.Core.Storage;
public interface IFileManager
{
    /// <summary>
    /// Loads the entries from the file. If the file does not exist, it creates a new one.
    /// </summary>
    /// <param name="masterKey"> The master key used for decryption.</param>
    /// <returns> A list of entries.</returns>
    /// <param name="filePath"> The path to the file.</param>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the file cannot be loaded or decrypted.</exception>
    Task<List<EntryModel>> LoadAsync(string masterKey, string filePath = "");

    /// <summary>
    /// Saves the entries to the file.
    /// </summary>
    /// <param name="entries"> The list of entries to save.</param>
    /// <param name="masterKey"> The master key used for encryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException"> Thrown when the master key is null or empty.</exception>
    /// <exception cref="InvalidOperationException"> Thrown when the file cannot be saved or encrypted.</exception>
    Task SaveAsync(List<EntryModel> entries, string masterKey, string filePath = "");
}