using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Core.Models;
using RingBearer.Mobile.Constants;
using RingBearer.Mobile.Models;
using RingBearer.Mobile.Services;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;
public partial class EntryViewModel(IStringLocalizer localizer, IDataAccess dataAccess) : ObservableObject, IEntryViewModel
{
    #region UI Messages

    public string KeyPlaceholder => localizer["KeyPlaceholderMAUI"];
    public string UserNamePlaceholder => localizer["UserNamePlaceholderMAUI"];
    public string PasswordPlaceholder => localizer["PasswordPlaceHolderMAUI"];
    public string NotesPlaceholder => localizer["NotesPlaceholderMAUI"];
    public string SaveButtonText => localizer["SaveButtonTextMAUI"];

    #endregion

    #region Attributes

    private string _entryPageTitle = localizer["AddEntryPageTitleMAUI"];

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isAddNewMode = true;
    private bool _isRefreshing = false;

    #endregion

    #region Properties

    public EntryDTO Model { get; set; } = new EntryDTO();

    public string EntryPageTitle
    {
        get => _entryPageTitle;
        set => SetProperty(ref _entryPageTitle, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(IsErrorMessageVisible));
        }
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            SetProperty(ref _successMessage, value);
            OnPropertyChanged(nameof(IsSuccessMessageVisible));
        }
    }

    public bool IsAddNewMode
    {
        get => _isAddNewMode;
        set => SetProperty(ref _isAddNewMode, value);
    }

    public bool IsErrorMessageVisible => !string.IsNullOrEmpty(_errorMessage);

    public bool IsSuccessMessageVisible => !string.IsNullOrEmpty(_successMessage);

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            SetProperty(ref _isRefreshing, value); // Notificás que cambió la propiedad
            OnPropertyChanged(nameof(IsSaveButtonEnabled));
        }
    }

    public bool IsSaveButtonEnabled => string.IsNullOrEmpty(_errorMessage) && !IsRefreshing;

    #endregion

    public void LoadEntry(string key)
    {
        ResponseDTO response = dataAccess.GetEntry(key);
        if (response.IsSuccess && response.Data is EntryDTO entry)
        {
            EntryPageTitle = localizer["EditEntryPageTitleMAUI"];
            Model = entry;
            OnPropertyChanged(nameof(Model)); // Notificás que cambió el modelo
            _isAddNewMode = false;
            return;
        }

        ErrorMessage = response.Message;
    }

    public async Task SaveEntryAsync()
    {
        if (string.IsNullOrEmpty(Model.Key))
        {
            ErrorMessage = localizer["KeyNullError"];
            return;
        }

        IsRefreshing = true;
        await Task.Delay(200); // da tiempo a que se actualice la UI

        ResponseDTO response = _isAddNewMode
            ? await dataAccess.AddEntryAsync(Model)
            : await dataAccess.UpdateEntryAsync(Model);

        IsRefreshing = false;

        if (response.IsSuccess)
        {
            if (_isAddNewMode)
            {
                await Shell.Current.GoToAsync($"//{AppRoutes.EntriesPage}");
            }
            else
            {
                await ShowSuccessSaveAsync();
            }
            return;
        }

        ErrorMessage = response.Message;
    }

    public async Task CopyToClipboardAsync(string fieldText)
    {
        if (!string.IsNullOrWhiteSpace(fieldText))
        {
            await Clipboard.SetTextAsync(fieldText);
        }
    }

    private async Task ShowSuccessSaveAsync()
    {
        ErrorMessage = string.Empty;

        SuccessMessage = _isAddNewMode
            ? localizer["EntryAdded"]
            : localizer["EntryUpdated"];

        //Desaparecer el mensaje después de 2 segundos
        await Task.Delay(5000);
        SuccessMessage = string.Empty;
    }

}
