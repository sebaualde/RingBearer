using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Core.Models;
using RingBearer.Mobile.AppConfigurations;
using RingBearer.Mobile.Constants;
using RingBearer.Mobile.Services;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;

public partial class LoginViewModel(IStringLocalizer localizer, IDataAccess dataAccess) : ObservableObject, ILoginViewModel
{
    #region UI Messages

    public string LoginSubtitle => localizer["HeaderSubtitleMAUI"];
    public string PasswordPlaceholder => localizer["PasswordPlaceHolderMAUI"];
    public string ConfirmPasswordPlaceHolder => localizer["ConfirmPasswordPlaceHolderMAUI"];
    public string LoginButtonText => localizer["LoginBtnText"];
    public string WarningText => localizer["PasswordWarningText"];

    #endregion

    #region Attributes

    private string _password = string.Empty;
    private string _passwordError = string.Empty;
    private bool _isPasswordHidden = true;

    private string _confrimPassword = string.Empty;
    private string _confirmPasswordError = string.Empty;
    private bool _isConfirmPasswordHidden = true;

    private bool _isRefreshing = false;
    private bool _isFirstLogin = dataAccess.FileExist() is false;
    private string _loginError = string.Empty;

    #endregion

    #region Properties

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            if (_isFirstLogin) ValidatePassword();
            OnPropertyChanged(nameof(IsLoginButtonEnabled));
        }
    }

    public string PasswordError
    {
        get => _passwordError;
        set => SetProperty(ref _passwordError, value);
    }

    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set => SetProperty(ref _isPasswordHidden, value);
    }


    public string ConfirmPassword
    {
        get => _confrimPassword;
        set
        {
            SetProperty(ref _confrimPassword, value);
            if (_isFirstLogin) ValidateConfirmPassword();
            OnPropertyChanged(nameof(IsLoginButtonEnabled));
        }
    }

    public bool IsConfirmPasswordHidden
    {
        get => _isConfirmPasswordHidden;
        set => SetProperty(ref _isConfirmPasswordHidden, value);
    }

    public string ConfirmPasswordError
    {
        get => _confirmPasswordError;
        set => SetProperty(ref _confirmPasswordError, value);
    }


    public string LoginError
    {
        get => _loginError;
        set
        {
            SetProperty(ref _loginError, value);
            OnPropertyChanged(nameof(IsLoginErrorVisible));
        }
    }

    public bool IsLoginErrorVisible => !string.IsNullOrEmpty(_loginError);

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            SetProperty(ref _isRefreshing, value);
            OnPropertyChanged(nameof(IsLoginButtonEnabled));
        }
    }

    public bool IsLoginButtonEnabled
    {
        get
        {
            return _isFirstLogin
                ? ValidatePassword() && ValidateConfirmPassword() && !IsRefreshing
                : !string.IsNullOrWhiteSpace(Password) && !IsRefreshing;
        }
    }

    public bool IsFirsrtLogin
    {
        get => _isFirstLogin;
        set => SetProperty(ref _isFirstLogin, value);
    }

    #endregion

    public async Task<ResponseDTO> LoginUserAsync()
    {
        LoginError = string.Empty;

        IsRefreshing = true;
        await Task.Delay(200); // da tiempo a que se actualice la UI

        ResponseDTO response = await dataAccess.LoginAsync(_password);
        if (response.IsSuccess)
        {
            ClearMessages();
            Preferences.Set(PreferencesNames.IsAuthenticated, true);
            Preferences.Set(PreferencesNames.IsFirstTime, false);
            IsFirsrtLogin = false;
        }

        IsRefreshing = false;

        LoginError = response.Message;
        return response;
    }

    private void ClearMessages()
    {
        Password = string.Empty;
        PasswordError = string.Empty;
        ConfirmPassword = string.Empty;
        ConfirmPasswordError = string.Empty;
        LoginError = string.Empty;
    }

    #region Validations

    /// <summary>
    /// Validates the password field (return true if valid).
    /// </summary>
    /// <returns></returns>
    private bool ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = string.IsNullOrWhiteSpace(ConfirmPassword)
                ? string.Empty
                : localizer["PasswordRequiredMAUI"];
            return false;
        }

        if (Password.Length < 3)
        {
            PasswordError = localizer["PasswordLengthMAUI"];
            return false;
        }

        if (!string.IsNullOrWhiteSpace(ConfirmPassword) && !Password.Equals(ConfirmPassword))
        {
            PasswordError = localizer["PasswordMatchMAUI"];
            return false;
        }

        ConfirmPasswordError = Password.Equals(ConfirmPassword)
         ? string.Empty
         : localizer["PasswordMatchMAUI"];

        PasswordError = string.Empty;
        return true;
    }

    /// <summary>
    /// Validates the confirm password field (return true if valid).
    /// </summary>
    private bool ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = string.IsNullOrWhiteSpace(Password)
                ? string.Empty
                : localizer["ConfirmPasswordRequiredMAUI"];
            return false;
        }
        if (ConfirmPassword.Length < 3)
        {
            ConfirmPasswordError = localizer["ConfirmPasswordLengthMAUI"];
            return false;
        }
        if (!string.IsNullOrWhiteSpace(Password) && !Password.Equals(ConfirmPassword))
        {
            ConfirmPasswordError = localizer["PasswordMatchMAUI"];
            return false;
        }

        PasswordError = Password.Equals(ConfirmPassword)
       ? string.Empty
       : localizer["PasswordMatchMAUI"];

        ConfirmPasswordError = string.Empty;
        return true;

    }

    #endregion
}
