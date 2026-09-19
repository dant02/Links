# Hello World Windows Widget

A small Windows 11-style, always-on-top widget written in C# and WPF. It displays **Hello world**, can be dragged around the desktop, and has a close button.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on the development machine
- Windows 11 to run the widget

The project targets Windows WPF. It can be built and published on Linux, but its user interface runs only on Windows.

## Build

From the repository root, restore dependencies and compile the application:

```bash
dotnet build HelloWorldWidget/HelloWorldWidget.csproj
```

The debug build output is written to `HelloWorldWidget/bin/Debug/net10.0-windows/`.

## Run on Windows

On a Windows machine with the .NET 10 Desktop Runtime or SDK installed, run:

```powershell
dotnet run --project .\HelloWorldWidget\HelloWorldWidget.csproj
```

## Publish a standalone Windows executable

Build a self-contained 64-bit Windows release from Linux or Windows:

```bash
dotnet publish HelloWorldWidget/HelloWorldWidget.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true
```

Copy the contents of `HelloWorldWidget/bin/Release/net10.0-windows/win-x64/publish/` to a Windows 11 computer and run `HelloWorldWidget.exe`.
