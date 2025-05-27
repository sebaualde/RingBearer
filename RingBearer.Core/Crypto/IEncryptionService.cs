using RingBearer.Core.Models;

namespace RingBearer.Core.Crypto;

public interface IEncryptionService
{
    /// <summary>
    /// Load entries from the encrypted file.
    /// </summary>
    /// <param name="masterKey"> The master key used for decryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> A list of EntryModel objects.</returns>
    Task<List<EntryModel>> LoadEntriesAsync(string masterKey, string filePath = "");

    /// <summary>
    /// Encrypts the given plain text using AES encryption with a password.
    /// </summary>
    /// <param name="password"> The password used for encryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> An array of bytes representing the encrypted data.</returns>
    /// <exception cref="ArgumentException"> Thrown when plainText or password is null or empty.</exception>
    Task<string> DecryptAsync(string password, string filePath = "");

    /// <summary>
    /// Decrypts the encrypted file using AES encryption with a password.
    /// </summary>
    /// <param name="plainText"> The plain text to encrypt.</param>
    /// <param name="password"> The password used for decryption.</param>
    /// <returns> The decrypted JSON string.</returns>
    /// <exception cref="ArgumentException"> Thrown when password is null or empty.</exception>
    byte[] Encrypt(string plainText, string password);
}