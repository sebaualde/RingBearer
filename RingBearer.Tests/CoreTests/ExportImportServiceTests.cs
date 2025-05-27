using Microsoft.Extensions.Localization;
using Moq;
using RingBearer.Core.Crypto;
using RingBearer.Core.Models;
using RingBearer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RingBearer.Tests.CoreTests;

public class ExportImportServiceTests
{
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<IStringLocalizer> _localizerMock = new();
    private readonly FileExchangeService _service;

    private readonly string _testPassword = "TestPassword";
    private readonly string _tempFile;

    private readonly List<EntryModel> _sampleEntries = new()
    {
        new EntryModel { Key = "testKey", UserName = "User", Password = "1234", Notes = "Some notes tests." }
    };

    public ExportImportServiceTests()
    {
        _service = new FileExchangeService(_encryptionServiceMock.Object, _localizerMock.Object);
        _tempFile = Path.GetTempFileName();
    }

    [Fact]
    public async Task ExportAsync_WritesPlainJson()
    {
        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.PlainJson
        };

        await _service.ExportAsync(_sampleEntries, options);

        var content = await File.ReadAllTextAsync(_tempFile);
        Assert.Contains("testKey", content);
        Assert.Contains("User", content);
        Assert.Contains("1234", content);
        Assert.Contains("Some notes tests.", content);
    }

    [Fact]
    public async Task ExportAsync_WritesEncryptedBinary()
    {
        var encryptedData = Encoding.UTF8.GetBytes("ENCRYPTED");
        _encryptionServiceMock
            .Setup(e => e.Encrypt(It.IsAny<string>(), _testPassword))
            .Returns(encryptedData);

        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.EncryptedBinary,
            Password = _testPassword
        };

        await _service.ExportAsync(_sampleEntries, options);

        var actualData = await File.ReadAllBytesAsync(_tempFile);
        Assert.Equal(encryptedData, actualData);
    }

    [Fact]
    public async Task ImportAsync_ReadsPlainJson()
    {
        var json = JsonSerializer.Serialize(_sampleEntries);
        await File.WriteAllTextAsync(_tempFile, json);

        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.PlainJson
        };

        var result = await _service.ImportAsync(options);

        Assert.Single(result);
        var entry = result[0];
        Assert.Equal("testKey", entry.Key);
        Assert.Equal("User", entry.UserName);
        Assert.Equal("1234", entry.Password);
        Assert.Equal("Some notes tests.", entry.Notes);
    }

    [Fact]
    public async Task ImportAsync_ReadsEncryptedBinary()
    {
        var json = JsonSerializer.Serialize(_sampleEntries);
        var fakeEncryptedData = Encoding.UTF8.GetBytes("FAKE BINARY");
        await File.WriteAllBytesAsync(_tempFile, fakeEncryptedData);

        _encryptionServiceMock
            .Setup(e => e.DecryptAsync(_testPassword, _tempFile))
            .ReturnsAsync(json);

        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.EncryptedBinary,
            Password = _testPassword
        };

        var result = await _service.ImportAsync(options);

        Assert.Single(result);
        var entry = result[0];
        Assert.Equal("testKey", entry.Key);
        Assert.Equal("User", entry.UserName);
        Assert.Equal("1234", entry.Password);
        Assert.Equal("Some notes tests.", entry.Notes);
    }

    [Fact]
    public async Task ExportAsync_ThrowsIfPasswordMissingForEncryption()
    {
        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.EncryptedBinary,
            Password = null
        };
        _localizerMock.Setup(l => l["PasswordRequiredForEncryptedExportException"]).Returns(new LocalizedString("PasswordRequiredForEncryptedExportException", "Password is required for encrypted export."));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportAsync(_sampleEntries, options));
        Assert.Contains("Password", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExportAsync_ThrowsIfOptionsIsNull()
    {
        ExportOptions? options = null;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ExportAsync(_sampleEntries, options!));
    }

    [Fact]
    public async Task ExportAsync_ThrowsIfFormatIsUnsupported()
    {
        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = (ExportFormat)999, // formato no definido
            Password = null
        };

        _localizerMock.Setup(l => l["ExportFormatNotSupportedException"])
                      .Returns(new LocalizedString("ExportFormatNotSupportedException", "Unsupported export format"));

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _service.ExportAsync(_sampleEntries, options));
    }

    [Fact]
    public async Task ImportAsync_ThrowsIfDeserializationFails()
    {
        // Archivo JSON inválido (por ejemplo, datos corruptos)
        await File.WriteAllTextAsync(_tempFile, "Not a valid json");

        var options = new ExportOptions
        {
            FilePath = _tempFile,
            Format = ExportFormat.PlainJson
        };

        _localizerMock.Setup(l => l["DeserializationFailedException"])
                      .Returns(new LocalizedString("DeserializationFailedException", "Deserialization failed. The file might be corrupted or in an invalid format."));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ImportAsync(options));

        Assert.Contains("Deserialization failed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }


    private void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

}
