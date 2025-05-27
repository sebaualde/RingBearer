using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;
using RingBearer.Core.Services;
using RingBearer.Mobile.ViewModels.Interfaces;
using System.Diagnostics;

namespace RingBearer.Mobile.ViewModels;
public class ImportExportViewModel(
    IFileExchangeService exchangeService,
    IEntriesManager entriesManager,
    IStringLocalizer localizer) : ObservableObject, IImportExportViewModel
{

    private static readonly Dictionary<DevicePlatform, IEnumerable<string>> _supportedFileTypes = new()
    {
        { DevicePlatform.Android, new[] { "application/json" } },
        { DevicePlatform.iOS, new[] { ".json" } },
        { DevicePlatform.WinUI, new[] { ".json" } }
    };

    #region UI Messages

    public string ExportSubtitle => localizer["ExportSubtitleMAUI"];
    public string ExportPlaceHolder => localizer["ExportPlaceHolderMAUI"];
    public string ExportButtonText => localizer["ExportButtonTextMAUI"];

    public string ImportSubtitle => localizer["ImportSubtitleMAUI"];
    public string ImportPlaceHolder => localizer["ImportPlaceHolderMAUI"];
    public string ImportButtonText => localizer["ImportButtonTextMAUI"];

    public string WarningImportText => localizer["WarningImportTextMAUI"];

    #endregion

    #region Attributes

    private string _exportPassword = string.Empty;
    private bool _isExportPasswordHidden = true;

    private string _importPassword = string.Empty;
    private bool _isImportPasswordHidden = true;

    #endregion

    #region Properties

    public string ExportPassword
    {
        get => _exportPassword;
        set => SetProperty(ref _exportPassword, value);
    }

    public bool IsExportPasswordHidden
    {
        get => _isExportPasswordHidden;
        set => SetProperty(ref _isExportPasswordHidden, value);
    }

    public string ImportPassword
    {
        get => _importPassword;
        set => SetProperty(ref _importPassword, value);
    }

    public bool IsImportPasswordHidden
    {
        get => _isImportPasswordHidden;
        set => SetProperty(ref _isImportPasswordHidden, value);
    }


    #endregion


    public async Task ExportAsync()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.CacheDirectory, $"export.json");
            ExportOptions options = new() { FilePath = filePath, Password = ExportPassword };

            IEnumerable<EntryModel> entries = entriesManager.GetEntries();
            if (entries == null || !entries.Any())
            {
                await Shell.Current.DisplayAlert(
                    localizer["DisplayAlertErrorTitleMAUI"],
                    localizer["NoEntriesToExportMessageMAUI"],
                    localizer["DisplayAlertOkButtonTextMAUI"]);

                return;
            }
            await exchangeService.ExportAsync(entries, options);

            string? folderPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                await Shell.Current.DisplayAlert(
                    localizer["DisplayAlertErrorTitleMAUI"],
                    localizer["ExportErrorMessageMAUI"],
                    localizer["DisplayAlertOkButtonTextMAUI"]);
                return;
            }

#if WINDOWS
            Process.Start("explorer.exe", folderPath);
#elif MACCATALYST
            Process.Start("open", folderPath);
#elif ANDROID || IOS

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = localizer["ExportedFilePathTitleMAUI"],
                File = new ShareFile(filePath)
            });
#else
            await Shell.Current.DisplayAlert("Info", "Opening folders is not supported on this platform.", "OK");
#endif

            return;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                localizer["DisplayAlertErrorTitleMAUI"],
                ex.Message,
                localizer["DisplayAlertOkButtonTextMAUI"]);

            return;
        }
    }

    public async Task ImportAsync()
    {
        try
        {
            // Check if the file is selected
            ResponseDTO fileSelected = await CheckFileSelectedAsync();
            if (!fileSelected.IsSuccess) return;

            // Get the file path and extension
            FileResult file = (FileResult)fileSelected.Data!;
            ResponseDTO supertedResponse = await CheckIfIsSupportedFileAsync(file.FullPath);
            if (!supertedResponse.IsSuccess) return;

            // Create the import entries list
            ResponseDTO entriesResponse = await GetEntriesListToImportAsync(file.FullPath);
            if (!entriesResponse.IsSuccess) return;

            // Import the entries
            List<EntryModel> entries = (List<EntryModel>)entriesResponse.Data!;
            await entriesManager.ImportEntriesAsync(entries);

            await Shell.Current.DisplayAlert(
                localizer["DisplayAlertInfoTitleMAUI"],
                localizer["EntriesImportedMAUI", entries.Count],
                localizer["DisplayAlertOkButtonTextMAUI"]);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    /// <summary>
    /// Check if the file is selected
    /// </summary>
    /// <returns> ResponseDTO with the file result or null if no file is selected</returns>
    private async Task<ResponseDTO> CheckFileSelectedAsync()
    {
        // if the platform is Android, check if the permission is granted
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            PermissionStatus status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert(
                    localizer["FileStoragePermisionTitleMAUI"],
                    localizer["FileStoragePerminsionTextMAUI"], 
                    localizer["DisplayAlertOkButtonTextMAUI"]);

                return new ResponseDTO { IsSuccess = false };
            }
        }

        FileResult? result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = new FilePickerFileType(_supportedFileTypes)
        });

        if (result == null)
        {
            await Shell.Current.DisplayAlert(
                localizer["DisplayAlertCancelTitleMAUI"],
                localizer["NoFileSelectedTextMAUI"],
                localizer["DisplayAlertOkButtonTextMAUI"]);

            return new ResponseDTO { IsSuccess = false };
        }

        return new ResponseDTO { IsSuccess = true, Data = result };
    }

    /// <summary>
    /// Check if the file is supported
    /// </summary>
    /// <param name="fullPath"> The full path of the file</param>
    /// <returns> ResponseDTO with the file result or null if no file is selected</returns>
    private async Task<ResponseDTO> CheckIfIsSupportedFileAsync(string fullPath)
    {
        bool isSupported = false;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            isSupported = Path.GetExtension(fullPath).Equals(".json", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // En otras plataformas, compara la extensión
            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            isSupported = _supportedFileTypes[DeviceInfo.Platform]
                .Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        if (!isSupported)
        {
            await Shell.Current.DisplayAlert(
                localizer["DisplayAlertErrorTitleMAUI"],
                localizer["UnsupportedFileErrorMAUI"],
                localizer["DisplayAlertOkButtonTextMAUI"]);
            return new ResponseDTO { IsSuccess = false };
        }
        return new ResponseDTO { IsSuccess = true };
    }

    /// <summary>
    /// Get the entries list to import
    /// </summary>
    /// <param name="fullPath"> The full path of the file</param>
    /// <returns> ResponseDTO with the entries list or null if no entries are found</returns>
    private async Task<ResponseDTO> GetEntriesListToImportAsync(string fullPath)
    {
        ExportOptions options = new() { FilePath = fullPath, Password = ImportPassword };
        List<EntryModel> entries = await exchangeService.ImportAsync(options);

        if (entries == null || entries.Count < 1)
        {
            await Shell.Current.DisplayAlert(
                localizer["DisplayAlertInfoTitleMAUI"],
                localizer["NoEntriesToImportMAUI"],
                localizer["DisplayAlertOkButtonTextMAUI"]);

            return new ResponseDTO { IsSuccess = false };
        }

        return new ResponseDTO { IsSuccess = true, Data = entries };
    }
}