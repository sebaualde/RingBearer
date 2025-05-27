using Microsoft.Extensions.Localization;
using RingBearer.Core.Constants;
using RingBearer.Core.Crypto;
using RingBearer.Core.Manager;
using RingBearer.Core.Models;
using RingBearer.Core.Services;
using RingBearer.Core.Storage;
using RingBearer.Mobile.Mapper;
using RingBearer.Mobile.Services;
using RingBearer.Mobile.ViewModels;
using RingBearer.Mobile.ViewModels.Interfaces;
using RingBearer.Mobile.Views;

namespace RingBearer.Mobile.AppConfigurations;

public static class DependenciesConfig
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        // localización
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Configura la ubicación de los recursos de localización (idiomas)
        builder.Services.AddSingleton(sp =>
        {
            IStringLocalizerFactory factory = sp.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create(AppConstants.BaseName, AppConstants.Location);
        });

        // Configuraciones de usuario (Theme, Language, etc.)
        builder.Services.AddSingleton<IUserConfigurations, UserConfigurations>();

        // Servicios de Core
        builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
        builder.Services.AddSingleton<IFileManager, FileManager>();
        builder.Services.AddSingleton<IEntriesManager, EntriesManager>();
        builder.Services.AddSingleton<IEntriesMapper, EntriesMapper>();
        builder.Services.AddSingleton<IDataAccess, DataAccess>();
        builder.Services.AddTransient<IEntryModel, EntryModel>();
        builder.Services.AddSingleton<IFileExchangeService, FileExchangeService>();

        // ViewModels 
        builder.Services.AddTransient<IShellViewModel, ShellViewModel>();
        builder.Services.AddTransient<ILoginViewModel, LoginViewModel>();
        builder.Services.AddTransient<IEntriesViewModel, EntriesViewModel>();
        builder.Services.AddTransient<IEntryViewModel, EntryViewModel>();
        builder.Services.AddTransient<ILanguageViewModel, LanguageViewModel>();
        builder.Services.AddTransient<IPasswordViewModel, PasswordViewModel>();
        builder.Services.AddTransient<IImportExportViewModel, ImportExportViewModel>();
        builder.Services.AddTransient<IAboutViewModel, AboutViewModel>();

        // Pages
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<EntriesPage>();
        builder.Services.AddTransient<EntryPage>();
        builder.Services.AddTransient<ImportExportPage>();
        builder.Services.AddTransient<LanguagePage>();
        builder.Services.AddTransient<PasswordPage>();
        builder.Services.AddTransient<AboutPage>();

        return builder;
    }
}

