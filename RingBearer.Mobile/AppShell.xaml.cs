using RingBearer.Mobile.Constants;
using RingBearer.Mobile.ViewModels.Interfaces;
using RingBearer.Mobile.Views;

namespace RingBearer.Mobile;

public partial class AppShell : Shell
{
    #region Campos privados

    private readonly IShellViewModel _shellViewModel;

    #endregion

    #region Constructor

    public AppShell()
    {
        InitializeComponent();
        BindingContext = _shellViewModel = App.Services.GetRequiredService<IShellViewModel>();

        UpdateFlyoutItems();
        RegisterRoutes();
        this.Navigated += OnShellNavigated; // para actualizar el título de la página actual
    }

    #endregion

    #region Routes Configuration

    /// <summary>
    /// Actualiza los elementos del menú de navegación (flyout) según el estado de autenticación del usuario.
    /// </summary>
    public void UpdateFlyoutItems()
    {
        bool isAuthenticated = Preferences.Get(PreferencesNames.IsAuthenticated, false);

        Items.Clear();

        if (isAuthenticated)
        {
            Items.Add(CreateEntriesPage());
            Items.Add(CreateLanguagePage());
            Items.Add(CreateChangePasswordPage());
            Items.Add(CreateAboutPage());
            _shellViewModel.ShowLogoutButton = true;
        }
        else
        {
            Items.Add(CreateLoginPage());
            _shellViewModel.ShowLogoutButton = false;
        }
    }

    /// <summary>
    /// Registra las rutas de navegación para las páginas de la aplicación.
    /// </summary>
    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(AppRoutes.LoginPage, typeof(LoginPage));
        Routing.RegisterRoute(AppRoutes.EntryPage, typeof(EntryPage));
    }

    /// <summary>
    /// Crea la página de inicio de sesión.
    /// </summary>
    /// <returns> Una instancia de ShellContent que representa la página de inicio de sesión.</returns>
    private ShellContent CreateLoginPage()
    {
        return new ShellContent
        {
            Title = _shellViewModel.LoginPageTitle,
            Icon = _shellViewModel.IsDarkTheme ? "login.png" : "logindark.png",
            ContentTemplate = new DataTemplate(typeof(LoginPage)),
            Route = AppRoutes.LoginPage
        };
    }

    private FlyoutItem CreateEntriesPage()
    {
        return new FlyoutItem
        {
            Title = _shellViewModel.EntriesPageTitle,
            Route = AppRoutes.EntriesPage,
            Icon = _shellViewModel.IsDarkTheme ? "entry.png" : "entrydark.png",
            Items =
            {
                new ShellContent
                {
                    Title=_shellViewModel.EntriesPageTitle,
                    Icon = _shellViewModel.IsDarkTheme ? "entry.png" : "entrydark.png",
                    ContentTemplate = new DataTemplate(typeof(EntriesPage)),
                    Route = AppRoutes.EntriesPage
                },
                new ShellContent
                {
                    Title = _shellViewModel.ImportExportPageTitle,
                    Icon = _shellViewModel.IsDarkTheme ? "importexport.png" : "importexportdark.png",
                    ContentTemplate = new DataTemplate(typeof(ImportExportPage)),
                    Route = AppRoutes.ImportExportPage
                },
            }
        };
    }

    private ShellContent CreateLanguagePage()
    {
        return new ShellContent
        {
            Title = _shellViewModel.LanguagePageTitle,
            Icon = _shellViewModel.IsDarkTheme ? "language.png" : "languagedark.png",
            ContentTemplate = new DataTemplate(typeof(LanguagePage)),
            Route = AppRoutes.LanguagePage
        };
    }

    private ShellContent CreateChangePasswordPage()
    {
        return new ShellContent
        {
            Title = _shellViewModel.PasswordPageTitle,
            Icon = _shellViewModel.IsDarkTheme ? "key.png" : "keydark.png",
            ContentTemplate = new DataTemplate(typeof(PasswordPage)),
            Route = AppRoutes.PasswordPage
        };
    }

    private ShellContent CreateAboutPage()
    {
        return new ShellContent
        {
            Title = $"{_shellViewModel.AboutPageTitle} Ring Bearer",
            Icon = _shellViewModel.IsDarkTheme ? "sursoft.png" : "sursoftDark.png",
            ContentTemplate = new DataTemplate(typeof(AboutPage)),
            Route = AppRoutes.AboutPage
        };

    }

    #endregion

    #region Métodos de navegación

    /// <summary>
    /// Redirige a la pagina de Entradas de la aplicación luego de un login.
    /// </summary>
    public async Task NavigateAfterLoginAsync()
    {
        UpdateFlyoutItems();
        await GoToAsync($"//{AppRoutes.EntriesPage}");
    }

    /// <summary>
    /// Redirige a la pagina de Login.
    /// </summary>
    public async Task NavigateToLoginPageAsync()
    {
        Preferences.Set(PreferencesNames.IsAuthenticated, false);
        Shell.Current.FlyoutIsPresented = false; // Cierra el flyout
        UpdateFlyoutItems();
        await GoToAsync($"//{AppRoutes.LoginPage}");
    }

    /// <summary>
    /// Cambia el lenguaje de la aplicación.
    /// </summary>
    /// <param name="culture">Lenguaje seleccionada por el usuario (en, es, pt, it, etc.).</param>
    public void ChangeLanguage(string culture)
    {
        _shellViewModel.ChangeLanguage(culture);
        Application.Current!.MainPage = new AppShell();
    }

    /// <summary>
    /// Actualiza el título de la página actual en la barra de navegación.
    /// </summary>
    /// <param name="sender"> El objeto que envía el evento.</param>
    /// <param name="e"> Los argumentos del evento de navegación.</param>
    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (Shell.Current?.CurrentItem is ShellItem item && item.CurrentItem is ShellSection section)
        {
            if (BindingContext is IShellViewModel vm)
            {
                vm.CurrentPageTitle = section.Title;
            }
        }
    }

    /// <summary>
    /// Maneja el evento de clic en el botón de cerrar sesión en el menú de navegación.
    /// </summary>
    /// <param name="sender"> El objeto que envía el evento.</param>
    /// <param name="e"> Los argumentos del evento.</param>
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await NavigateToLoginPageAsync();
    }

    #endregion

    /// <summary>
    /// Maneja el evento de clic en el botón de alternar tema.
    /// </summary>
    /// <param name="sender"> El objeto que envía el evento.</param>
    /// <param name="e"> Los argumentos del evento.</param>
    private void OnToggleThemeClicked(object sender, EventArgs e)
    {
        _shellViewModel.ChangeAppTheme();
        Shell.Current.FlyoutIsPresented = false; // Cierra el flyout
        UpdateFlyoutItems();
    }
}
