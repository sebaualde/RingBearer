using System.Windows.Input;

namespace RingBearer.Mobile.ViewModels.Interfaces;
public interface IPasswordViewModel
{

    #region UI Messages

    string WarningText { get; }
    string SubtitleText { get; }

    string PasswordPlaceHolder { get; }
    string ConfirmPasswordPlaceHolder { get; }
    string SaveButtonText { get; }

    string ErrorMessage { get; }
    bool IsErrorMessageVisible { get; }

    string SuccessMessage { get; }
    bool IsSuccessMessageVisible { get; }

    #endregion

    string Password { get; set; }
    string PasswordError { get; }
    bool IsPasswordHidden { get; set; }

    string ConfirmPassword { get; set; }
    string ConfirmPasswordError { get; }
    bool IsConfirmPasswordHidden { get; set; }

    bool IsSaveButtonEnabled { get; }
    bool IsRefreshing { get; }

    Task SavePasswordAsync();

}