using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Microsoft.Windows.Widgets.Providers;

namespace Links;

[Guid("5B5A2943-3A52-4AE6-9D7B-8C811E342D12")]
internal sealed class WidgetProvider : IWidgetProvider
{
    // Must match com:Class Id and CreateInstance ClassId in Package/Package.appxmanifest.
    internal static readonly Guid ClassId = typeof(WidgetProvider).GUID;

    internal const string DefinitionId = "HelloWorldWidget";

    private static readonly ConcurrentDictionary<string, WidgetInfo> Widgets = new();
    private static readonly ManualResetEvent NoWidgetsRemaining = new(initialState: false);

    public WidgetProvider()
    {
        foreach (var info in WidgetManager.GetDefault().GetWidgetInfos())
        {
            var context = info.WidgetContext;
            Widgets.TryAdd(context.Id, new WidgetInfo(context.Id, context.DefinitionId));
        }
    }

    public void CreateWidget(WidgetContext widgetContext)
    {
        var widget = new WidgetInfo(widgetContext.Id, widgetContext.DefinitionId);
        Widgets[widget.Id] = widget;
        NoWidgetsRemaining.Reset();
        UpdateWidget(widget);
    }

    public void DeleteWidget(string widgetId, string customState)
    {
        Widgets.TryRemove(widgetId, out _);
        UpdateNoWidgetsEvent();
    }

    public void Activate(WidgetContext widgetContext)
    {
        if (Widgets.TryGetValue(widgetContext.Id, out var widget))
        {
            UpdateWidget(widget);
        }
    }

    public void Deactivate(string widgetId)
    {
        // No background work is performed for this static widget.
    }

    public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
    {
        switch (actionInvokedArgs.Verb)
        {
            case "open":
                Launch(ReadLinkId(actionInvokedArgs.Data));
                break;
            case "import":
                ImportLinks();
                break;
            case "export":
                ExportLinks();
                break;
        }
    }

    public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
    {
        if (Widgets.TryGetValue(contextChangedArgs.WidgetContext.Id, out var widget))
        {
            UpdateWidget(widget);
        }
    }

    internal static void WaitForAllWidgetsToBeRemoved() => NoWidgetsRemaining.WaitOne();

    private static void ImportLinks()
    {
        var path = NativeFileDialog.OpenXml();
        if (string.IsNullOrWhiteSpace(path) || !LinkStore.TryImport(path))
        {
            return;
        }

        UpdateAllWidgets();
    }

    private static void ExportLinks()
    {
        var path = NativeFileDialog.SaveXml();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        LinkStore.Export(path);
    }

    private static void Launch(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var link = LinkStore.Load().FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            return;
        }

        if (link.Type.Equals(LinkItem.TypeRdp, StringComparison.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "mstsc.exe",
                Arguments = $"/v:{link.Target}",
                UseShellExecute = true,
            });
            return;
        }

        if (link.Type.Equals(LinkItem.TypeBatch, StringComparison.OrdinalIgnoreCase))
        {
            var workingDirectory = string.IsNullOrWhiteSpace(link.WorkingDirectory)
                ? Path.GetDirectoryName(link.Target)
                : link.WorkingDirectory;
            var keep = link.KeepWindowOpen ? "/k" : "/c";

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"{keep} \"{link.Target}\"",
                WorkingDirectory = workingDirectory ?? string.Empty,
                UseShellExecute = true,
            });
        }
    }

    private static string? ReadLinkId(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(data)?["id"]?.GetValue<string>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void UpdateAllWidgets()
    {
        foreach (var widget in Widgets.Values)
        {
            UpdateWidget(widget);
        }
    }

    private static void UpdateWidget(WidgetInfo widget)
    {
        if (widget.DefinitionId != DefinitionId)
        {
            return;
        }

        var request = new WidgetUpdateRequestOptions(widget.Id)
        {
            Template = CardTemplate.Build(LinkStore.Load()),
            Data = "{}",
            CustomState = string.Empty,
        };

        WidgetManager.GetDefault().UpdateWidget(request);
    }

    private static void UpdateNoWidgetsEvent()
    {
        if (Widgets.IsEmpty)
        {
            NoWidgetsRemaining.Set();
        }
        else
        {
            NoWidgetsRemaining.Reset();
        }
    }

    private sealed record WidgetInfo(string Id, string DefinitionId);
}
