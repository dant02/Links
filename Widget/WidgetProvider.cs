using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Windows.Widgets.Providers;

namespace Links;

internal sealed class WidgetProvider : IWidgetProvider
{
    // This value must match both ClassId attributes in Widget.Package/Package.appxmanifest.
    internal static readonly Guid ClassId = Guid.Parse("5B5A2943-3A52-4AE6-9D7B-8C811E342D12");

    internal const string DefinitionId = "HelloWorldWidget";

    private const string Template = """
        {
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          "type": "AdaptiveCard",
          "version": "1.5",
          "body": [
            {
              "type": "TextBlock",
              "text": "Hello world",
              "size": "ExtraLarge",
              "weight": "Bolder",
              "wrap": true,
              "horizontalAlignment": "Center",
              "spacing": "Large"
            }
          ]
        }
        """;

    private static readonly ConcurrentDictionary<string, WidgetInfo> Widgets = new();
    private static readonly ManualResetEvent NoWidgetsRemaining = new(initialState: false);

    public WidgetProvider()
    {
        foreach (var info in WidgetManager.GetDefault().GetWidgetInfos())
        {
            var context = info.WidgetContext;
            Widgets.TryAdd(context.Id, new WidgetInfo(context.Id, context.DefinitionId));
        }

        UpdateNoWidgetsEvent();
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
        // The Hello world card has no actions.
    }

    public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
    {
        if (Widgets.TryGetValue(contextChangedArgs.WidgetContext.Id, out var widget))
        {
            UpdateWidget(widget);
        }
    }

    internal static void WaitForAllWidgetsToBeRemoved() => NoWidgetsRemaining.WaitOne();

    private static void UpdateWidget(WidgetInfo widget)
    {
        if (widget.DefinitionId != DefinitionId)
        {
            return;
        }

        var request = new WidgetUpdateRequestOptions(widget.Id)
        {
            Template = Template,
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
