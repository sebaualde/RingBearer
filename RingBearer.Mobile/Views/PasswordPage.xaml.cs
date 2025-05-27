using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class PasswordPage : ContentPage
{
    private readonly IPasswordViewModel _viewModel;

    public PasswordPage(IPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        _viewModel.IsPasswordHidden = !_viewModel.IsPasswordHidden;
        ChangeVisibilityIcon((ImageButton)sender, _viewModel.IsPasswordHidden);
    }

    private void OnToggleConfirmPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _viewModel.IsConfirmPasswordHidden = !_viewModel.IsConfirmPasswordHidden;
        ChangeVisibilityIcon((ImageButton)sender, _viewModel.IsConfirmPasswordHidden);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await _viewModel.SavePasswordAsync();
    }

    private static void ChangeVisibilityIcon(ImageButton button, bool isButtonHidden)
    {
        string eyeOff = App.Current!.UserAppTheme == AppTheme.Light ? "eyeoffdark.png" : "eyeoff.png";
        string eyeOn = App.Current!.UserAppTheme == AppTheme.Light ? "eyedark.png" : "eye.png";
        button.Source = isButtonHidden ? eyeOn : eyeOff;
    }
}