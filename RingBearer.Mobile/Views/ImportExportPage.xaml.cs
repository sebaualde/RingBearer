using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class ImportExportPage : ContentPage
{
    private readonly IImportExportViewModel _importExportViewModel;

    public ImportExportPage(IImportExportViewModel importExportViewModel)
    {
        InitializeComponent();

        BindingContext = _importExportViewModel = importExportViewModel;
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await _importExportViewModel.ExportAsync();
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        await _importExportViewModel.ImportAsync();
    }

    private void OnExportPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _importExportViewModel.IsExportPasswordHidden = !_importExportViewModel.IsExportPasswordHidden;
        ChangeVisibilityIcon((ImageButton)sender, _importExportViewModel.IsExportPasswordHidden);
    }

    private void OnImportPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _importExportViewModel.IsImportPasswordHidden = !_importExportViewModel.IsImportPasswordHidden;
        ChangeVisibilityIcon((ImageButton)sender, _importExportViewModel.IsImportPasswordHidden);
    }

    private static void ChangeVisibilityIcon(ImageButton button, bool isButtonHidden)
    {
        string eyeOff = App.Current!.UserAppTheme == AppTheme.Light ? "eyeoffdark.png" : "eyeoff.png";
        string eyeOn = App.Current!.UserAppTheme == AppTheme.Light ? "eyedark.png" : "eye.png";
        button.Source = isButtonHidden ? eyeOn : eyeOff;
    }

}