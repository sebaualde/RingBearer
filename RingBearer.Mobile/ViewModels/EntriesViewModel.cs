using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Mobile.Models;
using RingBearer.Mobile.Services;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;
public partial class EntriesViewModel(IStringLocalizer localizer, IDataAccess dataAccess) : ObservableObject, IEntriesViewModel
{
    private CancellationTokenSource? _debounceCts;

    #region UI Messages 

    public string SearchPlaceholder => localizer["SearchPlaceholder"];
    public string EntriesTitle => localizer["EntriesListTitleMAUI"];
    public string NoEntriesText => localizer["NoEntries"];
    public string SelectAllText => localizer["SelectAllTextMAUI"];
    public string DeleteSelectedButtonText => localizer["DeleteSelectedButtonTextMAUI"];

    #endregion

    #region Attributes

    private IEnumerable<EntryDTO> _entriesList = [];

    private string _searchTerm = string.Empty;
    private bool _isDeleteMode;
    private bool _areAllSelected;
    private bool _isRefreshing = false;

    #endregion

    #region Properties

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                _ = FilterEntriesAsync(_searchTerm);
            }
        }
    }

    public bool IsDeleteMode
    {
        get => _isDeleteMode;
        set => SetProperty(ref _isDeleteMode, value);
    }

    public bool AreAllSelected
    {
        get => _areAllSelected;
        set
        {
            if (_areAllSelected != value)
            {
                _areAllSelected = value;
                OnPropertyChanged(nameof(AreAllSelected));

                // Asignar el valor de IsSelected a cada entrada en la lista
                foreach (EntryDTO entry in EntriesList)
                {
                    entry.IsSelected = AreAllSelected;
                }
            }
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            SetProperty(ref _isRefreshing, value);
            OnPropertyChanged(nameof(ShowList));
        }
    }

    public bool ShowList => !_isRefreshing;

    public IEnumerable<EntryDTO> EntriesList
    {
        get => _entriesList;
        set => SetProperty(ref _entriesList, value);
    }

    #endregion

    public async Task LoadEntriesAsync()
    {
        IsRefreshing = true;
        await Task.Delay(200); // da tiempo a que se actualice la UI

        EntriesList = dataAccess.GetEntries();

        IsRefreshing = false;
    }

    public async Task DeleteEntriesAsync()
    {
        List<EntryDTO> selectedEntries = [.. EntriesList.Where(entry => entry.IsSelected)];
        if (selectedEntries.Count > 0)
        {
            await DeleteSelectedEntriesAsync(selectedEntries);
            return;
        }

        await Shell.Current.DisplayAlert(
            localizer["NoEntriesSelectedHeaderMAUI"],
            localizer["NoEntriesSelectedMessageMAUI"],
            localizer["NoEntriesSelectedButtonMAUI"]);
    }

    public void ToggleDeleteMode()
    {
        IsDeleteMode = !IsDeleteMode;
        if (IsDeleteMode is false)
        {
            AreAllSelected = false;
        }
        SetDeleteModeOnEntriesList();
    }

    public async Task InvertEntriesListAsync()
    {
        IsRefreshing = true;
        await Task.Delay(200); // da tiempo a que se actualice la UI

        EntriesList = [.. _entriesList.AsEnumerable().Reverse()];
        IsRefreshing = false;
    }

    public void OnSelectAllCheckedChanged()
    {
        _areAllSelected = !_areAllSelected;
        OnPropertyChanged(nameof(AreAllSelected));

        // Asignar el valor de IsSelected a cada entrada en la lista
        foreach (EntryDTO entry in EntriesList)
        {
            entry.IsSelected = AreAllSelected;
        }
    }

    private void SetDeleteModeOnEntriesList()
    {
        foreach (EntryDTO entry in EntriesList)
        {
            entry.IsDeleteMode = IsDeleteMode;
            if (IsDeleteMode is false) entry.IsSelected = false;
        }
    }

    private async Task FilterEntriesAsync(string texto)
    {
        _debounceCts?.Cancel(); // Cancelamos cualquier tarea previa en espera
        _debounceCts = new CancellationTokenSource();

        try
        {
            // Esperamos 500 ms antes de ejecutar el filtrado, permitiendo al usuario terminar de escribir
            await Task.Delay(TimeSpan.FromMilliseconds(500), _debounceCts.Token);

            if (!_debounceCts.Token.IsCancellationRequested)
            {
                IsRefreshing = true;
                await Task.Delay(200);

                EntriesList = dataAccess.FilterEntries(texto);
                IsRefreshing = false;
            }
        }
        catch (TaskCanceledException)
        {
            // Ignoramos la excepción ya que es esperada cuando se cancela la tarea 
        }
    }

    private async Task DeleteSelectedEntriesAsync(List<EntryDTO> selectedEntries)
    {
        bool isConfirmed = await RequestConfirmationForDeleteAsync();

        if (isConfirmed)
        {
            IsRefreshing = true;
            await Task.Delay(200); // da tiempo a que se actualice la UI

            Core.Models.ResponseDTO response = await dataAccess.DeleteSelectedEntriesAsync(selectedEntries);
            if (response.IsSuccess)
            {
                EntriesList = dataAccess.GetEntries();
                IsDeleteMode = false;
                AreAllSelected = false;
                IsRefreshing = false;
                return;
            }

            await ShowDeleteErrorAsync(response.Message);
        }
    }

    private async Task<bool> RequestConfirmationForDeleteAsync()
    {
        return await Shell.Current.DisplayAlert(
                localizer["ConfirmDeletionHeaderMAUI"],
                localizer["ConfirmDeletionMessageMAUI"],
                localizer["ConfirmDeletionYesButtonMAUI"],
                localizer["ConfirmDeletionNoButtonMAUI"]);
    }

    private async Task ShowDeleteErrorAsync(string message)
    {
        await Shell.Current.DisplayAlert(
            localizer["DisplayAlertErrorTitleMAUI"],
            message,
            localizer["DisplayAlertOkButtonTextMAUI"]);

        IsRefreshing = false;
    }

}
