using RingBearer.Core.Models;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ILoginViewModel _loginView;

    public LoginPage(ILoginViewModel loginView)
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false); //oculta la barra de navegacion en login
        BindingContext = _loginView = loginView;
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        _loginView.IsPasswordHidden = !_loginView.IsPasswordHidden;

        ChangeVisibilityIcon((ImageButton)sender, _loginView.IsPasswordHidden);
    }

    private void OnToggleConfirmPasswordVisibilityClicked(object sender, EventArgs e)
    {
        _loginView.IsConfirmPasswordHidden = !_loginView.IsConfirmPasswordHidden;

        ChangeVisibilityIcon((ImageButton)sender, _loginView.IsConfirmPasswordHidden);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ResponseDTO response = await _loginView.LoginUserAsync();
        if (response.IsSuccess)
        {
            if (App.Current!.MainPage is AppShell shell)
                await shell.NavigateAfterLoginAsync();
        }
    }

    private static void ChangeVisibilityIcon(ImageButton button, bool isButtonHidden)
    {
        string eyeOff = App.Current!.UserAppTheme == AppTheme.Light ? "eyeoffdark.png" : "eyeoff.png";
        string eyeOn = App.Current!.UserAppTheme == AppTheme.Light ? "eyedark.png" : "eye.png";
        button.Source = isButtonHidden ? eyeOn : eyeOff;
    }
}