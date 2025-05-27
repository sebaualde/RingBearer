using RingBearer.Core.Models;

namespace RingBearer.Mobile.ViewModels.Interfaces;
public interface ILoginViewModel
{
    #region UI Messages

    string PasswordPlaceholder { get; }
    string PasswordError { get; }
    bool IsPasswordHidden { get; set; }

    string ConfirmPasswordError { get; }
    bool IsConfirmPasswordHidden { get; set; }
    string ConfirmPasswordPlaceHolder { get; }

    string LoginError { get; }
    bool IsLoginErrorVisible { get; }

    string LoginButtonText { get; }
    string LoginSubtitle { get; }
    string WarningText { get; }

    #endregion

    string Password { get; set; }
    string ConfirmPassword { get; set; }

    bool IsRefreshing { get; }
    bool IsFirsrtLogin { get; }
    bool IsLoginButtonEnabled { get; }

    Task<ResponseDTO> LoginUserAsync();
}