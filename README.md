# Hello World Widgets Board provider

This is a Windows 11 **Widgets Board** provider. After it has been packaged, installed, and pinned, it appears in the board opened with `Win+W`; it is not a floating desktop window.

The provider returns a simple Adaptive Card whose content is **Hello world**.

## Project layout

- `Widget/` contains the C# out-of-process COM server that implements `IWidgetProvider`.
- `Widget.Package/` contains the MSIX package registration used by the Widgets Board to discover and activate the provider.

## Prerequisites

- .NET 10 SDK to compile the provider.
- Windows 11 with Developer Mode enabled to deploy it locally.
- Visual Studio 2022 or later on Windows, with the **WinUI application development** workload, to build and deploy the MSIX package.
- Four standard package logos in `Widget.Package/Assets/` and two picker images in `Widget.Package/ProviderAssets/`. See the `README.md` file in each folder for their names.

Only packaged applications can be registered as Widgets Board providers. The C# code can be compiled on Linux, but MSIX packaging and deployment must be performed on Windows.

## Build the provider on Linux

From the repository root:

```bash
dotnet build Widget/Widget.csproj -r win-x64
```

This validates and produces the Windows provider executable in `Widget/bin/Debug/net10.0-windows10.0.19041.0/win-x64/`.

## Package and test on Windows

1. Open `Links.slnx` in Visual Studio.
2. Add the required PNG logo and picker image files named in the two asset-folder READMEs.
3. Select the `x64` solution platform.
4. Build the solution, then right-click **Widget.Package** and choose **Deploy**. Visual Studio creates and installs a test-signed MSIX package.
5. Open the Widgets Board with `Win+W`, choose **Add widgets**, locate **Hello world**, and pin it.

The Widgets Board launches the provider when the widget is pinned and requests its card content; do not launch `Widget.exe` yourself.

## References

- [Implement a widget provider in a C# Windows app](https://learn.microsoft.com/windows/apps/develop/widgets/implement-widget-provider-cs)
- [Windows widget provider overview](https://learn.microsoft.com/windows/apps/develop/widgets/widget-providers)
