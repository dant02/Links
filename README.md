# Links widget for Windows 11 Widget board

This is a widget for Windows 11 **Widgets Board**. After it has been packaged, installed, and pinned, it appears in the board opened with `Win+W`.

The widget reads links from `%LOCALAPPDATA%\Links\links.xml` and shows each one as a clickable row on the widget card. **Import** and **Export** on the card open a file dialog so you can replace or copy that XML.

If the file is missing, it is created with two defaults: **RDP to 10.0.0.1** (`rdp`, launches `mstsc`) and **Build DB** (`batch`, runs `cmd /k` so the window stays open).

```xml
<links>
  <link id="rdp-lab" type="rdp" title="RDP to 10.0.0.1" target="10.0.0.1" />
  <link id="db" type="batch" title="Build DB"
        target="X:\workspace\build_db.bat"
        workingDirectory="X:\workspace"
        keepWindowOpen="true" />
</links>
```

`type` must be `rdp` or `batch`. When installed as MSIX, `%LOCALAPPDATA%` is the package’s private AppData; use Export if you want a copy you can edit elsewhere.

## Project layout

- `Widget/` contains the C# out-of-process COM server that implements `IWidgetProvider`.
- `Package/` contains the MSIX package registration used by the Widgets Board to discover and activate the provider.

## Prerequisites

- .NET 10 SDK to compile the provider.
- Windows 11 with Developer Mode enabled to deploy it locally.
- Visual Studio 2022 or later on Windows, with the **WinUI application development** workload, to build and deploy the MSIX package.
- Package logos in `Package/Images/`. The widget picker reuses `StoreLogo.png`, `Square150x150Logo.png`, and `Wide310x150Logo.png`.

Only packaged applications can be registered as Widgets Board providers. The C# code can be compiled on Linux, but MSIX packaging and deployment must be performed on Windows.

## Build the provider on Linux

From the repository root:

```bash
dotnet build Widget/Widget.csproj -r win-x64
```

This validates and produces the Windows provider executable in `Widget/bin/Debug/net10.0-windows10.0.19041.0/win-x64/`.

## Package and test on Windows

1. Open `Links.slnx` in Visual Studio.
2. Select the `x64` solution platform.
3. Build the solution, then right-click **Package** and choose **Deploy**. Visual Studio creates and installs a test-signed MSIX package.
4. Open the Widgets Board with `Win+W`, choose **Add widgets**, locate **Hello world**, and pin it.

The Widgets Board launches the provider when the widget is pinned and requests its card content; do not launch `Widget.exe` yourself.

## References

- [Implement a widget provider in a C# Windows app](https://learn.microsoft.com/windows/apps/develop/widgets/implement-widget-provider-cs)
- [Windows widget provider overview](https://learn.microsoft.com/windows/apps/develop/widgets/widget-providers)
