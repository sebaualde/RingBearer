using Microsoft.Extensions.Localization;
using RingBearer.Core.Constants;
using RingBearer.Core.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RingBearer.Core.Crypto;
public class EncryptionService : IEncryptionService
{
    private readonly IStringLocalizer _localizer;

    // private readonly string _filePath;

    public EncryptionService(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Load entries from the encrypted file.
    /// </summary>
    /// <param name="masterKey"> The master key used for decryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> A list of EntryModel objects.</returns>
    public async Task<List<EntryModel>> LoadEntriesAsync(string masterKey, string filePath = "")
    {
        // Desencriptar el archivo y leer el contenido
        string jsonString = await DecryptAsync(masterKey, filePath);
        List<EntryModel>? entries = JsonSerializer.Deserialize<List<EntryModel>>(jsonString,
        new JsonSerializerOptions
        {
            WriteIndented = true, // Formato legible 
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) // Convertir enums a camelCase
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Ignorar propiedades nulas
        });

        // Asegurarse de que entries no sea nulo
        return entries?.OrderBy(e => e.Key).ToList() ?? [];
    }

    /// <summary>
    /// Decrypts the encrypted file using AES encryption with a password.
    /// </summary>
    /// <param name="password"> The password used for decryption.</param>
    /// <param name="filePath"> The path to the file.</param>
    /// <returns> The decrypted JSON string.</returns>
    /// <exception cref="ArgumentException"> Thrown when password is null or empty.</exception>
    public async Task<string> DecryptAsync(string password, string filePath = "")
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(_localizer["PasswordNullError"], nameof(password));

        if (string.IsNullOrWhiteSpace(filePath))
            filePath = AppConstants.FileName; // Si no se proporciona una ruta de archivo, usa la predeterminada

        using FileStream fs = new(filePath, FileMode.Open); // Abre el archivo en modo lectura

        if (fs.Length == 0)
            throw new FileNotFoundException(_localizer["FileNotFound"]);

        byte[] salt = new byte[AppConstants.SaltSize]; // Crea un arreglo de bytes para el salt 
        byte[] iv = new byte[16]; // Crea un arreglo de bytes para el vector de inicialización (IV)

        fs.ReadExactly(salt); // Lee el salt del archivo
        fs.ReadExactly(iv); // Lee el IV del archivo

        byte[] key = GetKeyBytes(password, salt); // Obtiene la clave a partir de la contraseña maestra y el salt

        using Aes aes = Aes.Create(); // Crea una instancia de Aes para desencriptar los datos
        aes.Key = key; // Asigna la clave generada a la instancia de Aes
        aes.IV = iv; // Asigna el IV leído del archivo a la instancia de Aes

        using CryptoStream cryptoStream = new(fs, aes.CreateDecryptor(), CryptoStreamMode.Read); // Crea un flujo de desencriptación
        using StreamReader reader = new(cryptoStream); // Crea un lector para leer el flujo de desencriptación
        string jsonString = await reader.ReadToEndAsync(); // Lee el contenido del flujo de desencriptación y lo almacena en una cadena

        return jsonString;
    }

    /// <summary>
    /// Encrypts the given plain text using AES encryption with a password.
    /// </summary>
    /// <param name="plainText"> The plain text to encrypt.</param>
    /// <param name="password"> The password used for encryption.</param>
    /// <returns> An array of bytes representing the encrypted data.</returns>
    /// <exception cref="ArgumentException"> Thrown when plainText or password is null or empty.</exception>
    public byte[] Encrypt(string plainText, string password)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException(_localizer["PlainTextNullError"], nameof(plainText));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(_localizer["PasswordNullError"], nameof(password));

        using Aes aes = Aes.Create();
        aes.GenerateIV();

        // Use a fixed salt size for consistency
        byte[] salt = RandomNumberGenerator.GetBytes(AppConstants.SaltSize);

        // Derive the key using PBKDF2 with SHA256
        using Rfc2898DeriveBytes key = new(password, salt, AppConstants.Iterations, HashAlgorithmName.SHA256);
        aes.Key = key.GetBytes(AppConstants.KeySize);

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        using MemoryStream ms = new();

        // Write salt and IV to the output stream
        ms.Write(salt, 0, salt.Length);
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
        using (StreamWriter sw = new(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Genera una clave a partir de la contraseña y el salt utilizando PBKDF2.
    /// </summary>
    /// <param name="password"> La contraseña maestra.</param>
    /// <param name="salt"> El salt utilizado para la encriptación.</param>
    /// <returns> Un arreglo de bytes que representa la clave generada.</returns>
    private static byte[] GetKeyBytes(string password, byte[] salt)
    {
        using Rfc2898DeriveBytes pbkdf2 = new(password, salt, AppConstants.Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(AppConstants.KeySize);
    }
}
