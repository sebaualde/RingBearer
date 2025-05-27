using RingBearer.Mobile.Models;
using RingBearer.Mobile.ViewModels.Interfaces;

namespace RingBearer.Mobile.Views;

public partial class LanguagePage : ContentPage
{
    private readonly ILanguageViewModel _viewModel;
    private string _cultureSelected = string.Empty;

    public LanguagePage(ILanguageViewModel viewModel)
    {
        InitializeComponent();      
        BindingContext = _viewModel = viewModel;
        _viewModel.SetDefaultLanguage();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.SetDefaultLanguage();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is LanguageDTO selectedItem)
        {
            _cultureSelected = selectedItem.Code;
            _viewModel.ShowSelectedLanguage(_cultureSelected);
        }
    }

    private void OnApplyLangClicked(object sender, EventArgs e)
    {
        if (App.Current!.MainPage is AppShell shell)
            shell.ChangeLanguage(_cultureSelected);
    }
}