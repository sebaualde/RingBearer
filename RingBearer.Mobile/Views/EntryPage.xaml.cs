using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class EntryPage : ContentPage, IQueryAttributable
{
    private readonly IEntryViewModel _viewModel;

    public EntryPage(IEntryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        // Eliminar la ventana al presionar el boton de volver
        Shell.Current.Navigation.PopAsync();

        return true; // Indica que el evento ha sido manejado
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Key", out object? keyObj) && keyObj is string key)
        {
            _viewModel.LoadEntry(key); // Carga el Entry por clave
        }
    }

    #region Actions

    private async void OnKeyCopyClicked(object sender, EventArgs e)
    {
        await _viewModel.CopyToClipboardAsync(_viewModel.Model.Key);
    }

    private async void OnPasswordCopyClicked(object sender, EventArgs e)
    {
        await _viewModel.CopyToClipboardAsync(_viewModel.Model.Password);
    }

    private async void OnUserNameCopyClicked(object sender, EventArgs e)
    {
        await _viewModel.CopyToClipboardAsync(_viewModel.Model.UserName);
    }

    private async void OnNotesCopyClicked(object sender, EventArgs e)
    {
        await _viewModel.CopyToClipboardAsync(_viewModel.Model.Notes);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await _viewModel.SaveEntryAsync();
    }

    #endregion

}