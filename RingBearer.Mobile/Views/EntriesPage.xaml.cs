using Microsoft.Extensions.Localization;
using RingBearer.Mobile.Constants;
using RingBearer.Mobile.Models;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class EntriesPage : ContentPage
{
    private bool _isAscending = true;
    private readonly IEntriesViewModel _viewModel;

    public EntriesPage(IEntriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadEntriesAsync();
    }

    private async void OnSortClicked(object sender, EventArgs e)
    {
        await AnimateInvertIconAsync((ImageButton)sender);

        await _viewModel.InvertEntriesListAsync();
    }

    private async Task AnimateInvertIconAsync(ImageButton image)
    {
        // Animación de giro (mitad)
        await image.ScaleYTo(0, 100, Easing.CubicIn);

        // Alternar ícono y lógica
        _isAscending = !_isAscending;
        image.Source = _isAscending ? "sortinvert.png" : "sort.png";

        // Animación de regreso
        await image.ScaleYTo(1, 100, Easing.CubicOut);
    }

    #region Delete Actions

    private void OnSelectAllLabelTapped(object sender, EventArgs e)
    {
        SelectAllCheckbox.IsChecked = !SelectAllCheckbox.IsChecked;
    }

    private void OnToogleDeleteModeClicked(object sender, EventArgs e)
    {
        _viewModel.ToggleDeleteMode();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await _viewModel.DeleteEntriesAsync();
    }
    
    #endregion

    #region Navegation

    private async void OnImportExportClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{AppRoutes.ImportExportPage}");
    }

    private async void OnEditEntrySelected(object sender, SelectionChangedEventArgs e)
    {

        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is EntryDTO selectedItem)
        {
            ResetUserSelections(sender);
            Dictionary<string, object> parameters = new()
            {
                { "Key", selectedItem.Key }
            };
            await Shell.Current.GoToAsync($"{AppRoutes.EntryPage}", parameters);
        }
    }

    private async void OnAddNewEntryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{AppRoutes.EntryPage}");
    }

    private void ResetUserSelections(object sender)
    {
        _viewModel.IsDeleteMode = false;
        _viewModel.AreAllSelected = false;

        CollectionView collection = (CollectionView)sender;
        collection.SelectedItem = null;
    }

    #endregion

}