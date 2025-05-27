using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using RingBearer.Core.Models;
using RingBearer.Mobile.Services;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.ViewModels;

public partial class PasswordViewModel(
    IStringLocalizer localizer,
    IDataAccess dataAccess) : ObservableObject, IPasswordViewModel
{
    #region UI Messages

    public string WarningText => localizer["PasswordWarningText"];
    public string SubtitleText => localizer["ChangePasswordSubtitleTextMAUI"];
    public string PasswordPlaceHolder => localizer["PasswordPlaceHolderMAUI"];
    public string ConfirmPasswordPlaceHolder => localizer["ConfirmPasswordPlaceHolderMAUI"];
    public string SaveButtonText => localizer["ChangePasswordButtonTextMAUI"];

    #endregion

    #region Attributes

    private bool _isRefreshing = false;

    private string _password = string.Empty;
    private string _passwordError = string.Empty;
    private bool _isPasswordHidden = true;

    private string _confrimPassword = string.Empty;
    private string _confirmPasswordError = string.Empty;
    private bool _isConfirmPasswordHidden = true;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    #endregion

    #region Properties

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ValidatePassword();
            OnPropertyChanged(nameof(IsSaveButtonEnabled));
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
            ValidateConfirmPassword();
            OnPropertyChanged(nameof(IsSaveButtonEnabled));
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

    public bool IsSaveButtonEnabled =>
        string.IsNullOrEmpty(PasswordError) &&
        string.IsNullOrEmpty(ConfirmPasswordError) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) && !IsRefreshing;

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

    public bool IsErrorMessageVisible => !string.IsNullOrEmpty(_errorMessage);

    public bool IsSuccessMessageVisible => !string.IsNullOrEmpty(_successMessage);

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            SetProperty(ref _isRefreshing, value);
            OnPropertyChanged(nameof(IsSaveButtonEnabled));
        }
    }

    #endregion

    public void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = string.IsNullOrWhiteSpace(ConfirmPassword)
                ? string.Empty
                : localizer["PasswordRequiredMAUI"];
            return;
        }

        if (Password.Length < 3)
        {
            PasswordError = localizer["PasswordLengthMAUI"];
            return;
        }

        if (!string.IsNullOrWhiteSpace(ConfirmPassword) && !Password.Equals(ConfirmPassword))
        {
            PasswordError = localizer["PasswordMatchMAUI"];
            return;
        }

        ConfirmPasswordError = Password.Equals(ConfirmPassword)
         ? string.Empty
         : localizer["PasswordMatchMAUI"];

        PasswordError = string.Empty;
        return;
    }

    public void ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = string.IsNullOrWhiteSpace(Password)
                ? string.Empty
                : localizer["ConfirmPasswordRequiredMAUI"];
            return;
        }
        if (ConfirmPassword.Length < 3)
        {
            ConfirmPasswordError = localizer["ConfirmPasswordLengthMAUI"];
            return;
        }
        if (!string.IsNullOrWhiteSpace(Password) && !Password.Equals(ConfirmPassword))
        {
            ConfirmPasswordError = localizer["PasswordMatchMAUI"];
            return;
        }

        PasswordError = Password.Equals(ConfirmPassword)
       ? string.Empty
       : localizer["PasswordMatchMAUI"];

        ConfirmPasswordError = string.Empty;
        return;

    }

    public async Task SavePasswordAsync()
    {
        IsRefreshing = true;
        await Task.Delay(200); // da tiempo a que se actualice la UI

        ResponseDTO result = await dataAccess.ChangeMasterKeyAsync(Password);
        IsRefreshing = false;
        if (result.IsSuccess)
        {
            ClearMessages();
            SuccessMessage = localizer["MasterKeyChangedSuccessMAUI"];
            //Desaparecer el mensaje después de 2 segundos
            await Task.Delay(5000);
            SuccessMessage = string.Empty;
            return;
        }
        ErrorMessage = localizer["MasterKeyChangedErrorMAUI"];
    }

    private void ClearMessages()
    {
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
        PasswordError = string.Empty;
        ConfirmPasswordError = string.Empty;
    }
}
