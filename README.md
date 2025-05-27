# RingBearer

**RingBearer** es una aplicación multiplataforma desarrollada con .NET MAUI para la gestión segura de información y contraseñas. Soporta localización, múltiples plataformas y arquitectura MVVM.

---

## Características principales

- Gestión segura de entradas y contraseñas.
- Soporte multiplataforma: Windows, Android, iOS, Mac.
- Localización (multilenguaje).
- Importación y exportación de datos.
- Actualizaciones y enlaces a documentación, políticas y términos.

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 o superior con soporte para .NET MAUI
- Windows 10 19041 o superior (para escritorio)
- SDKs de Android/iOS instalados (para móviles)

---

## Instalación

1. **Clona el repositorio:**
	git clone https://github.com/tuusuario/ringbearer.git cd ringbearer


2. **Restaura los paquetes NuGet:**

   dotnet restore

3. **Compila la solución:**

   dotnet build

4. **Ejecuta la aplicación:**
   - Desde Visual Studio: selecciona la plataforma y pulsa **F5**.
   - Desde terminal:

    dotnet run --project RingBearer.Mobile

> ⚠️ **Advertencia importante**
>
> Si al compilar o ejecutar la aplicación en una plataforma móvil (Android/iOS) la app no inicia correctamente, primero intenta compilar y ejecutar la versión de Windows. Esto generará correctamente las carpetas `obj` y `bin` necesarias para el proyecto. Luego vuelve a intentar ejecutar la versión móvil.
>
> Actualmente, el proyecto solo ha sido probado en Windows y Android. El funcionamiento en otras plataformas (iOS, MacCatalyst, etc.) no está garantizado.

---

## Configuración de localización

- Los recursos de idioma están en `RingBearer.Core/Resources/Messages.resx` y sus variantes (`.es.resx`, `.it.resx`, `.pt.resx`).
- En `DependenciesConfig.cs` se configura la localización:

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

- Asegúrate de tener el paquete NuGet:

dotnet add package Microsoft.Extensions.Localization

---

## Estructura del proyecto

- **RingBearer.Core**: Lógica de negocio, servicios y recursos compartidos.
- **RingBearer.Mobile**: Proyecto principal MAUI (vistas, viewmodels, configuración).
- **Resources**: Archivos de recursos, estilos y localización.
- **Views**: Páginas de la interfaz de usuario.
- **ViewModels**: Lógica de presentación y binding.

---

## Publicar la aplicación (APK/.EXE)

### Windows (.exe portable)

Para generar una versión portable de la aplicación para Windows (solo el ejecutable):

1. Edita el archivo `RingBearer.Mobile.csproj` y descomenta la línea:

	`<WindowsPackageType>None</WindowsPackageType>`

   Esto deshabilita la generación del paquete MSIX y permite crear un `.exe` portable.

2. Publica la aplicación:

   El ejecutable estará en `bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`.

### Android (APK)

Para generar el archivo APK de Android:

	dotnet publish -f net8.0-android -c Release

El archivo `.apk` estará en `bin\Release\net8.0-android\publish\`.

**Nota:**  
Para depuración, vuelve a comentar la línea `<WindowsPackageType>None</WindowsPackageType>` en el `.csproj`.

---

## Enlaces útiles

- [Sobre Ring Bearer](https://ringbearer.sursoft.org/about)
- [Documentación](https://ringbearer.sursoft.org/documentation)
- [Política de privacidad](https://ringbearer.sursoft.org/privacy)
- [Términos y condiciones](https://ringbearer.sursoft.org/terms)
- [Comprobar actualizaciones](https://ringbearer.sursoft.org/download)
