using System.Text.Json;
using System.Text.Json.Nodes;

namespace Links;

internal static class CardTemplate
{
    internal static string Build(IReadOnlyList<LinkItem> links)
    {
        var body = new JsonArray();

        if (links.Count == 0)
        {
            body.Add(new JsonObject
            {
                ["type"] = "TextBlock",
                ["text"] = "No links. Use Import or add entries to links.xml.",
                ["wrap"] = true,
                ["horizontalAlignment"] = "Center",
            });
        }
        else
        {
            foreach (var link in links)
            {
                body.Add(new JsonObject
                {
                    ["type"] = "Container",
                    ["selectAction"] = new JsonObject
                    {
                        ["type"] = "Action.Execute",
                        ["verb"] = "open",
                        ["title"] = link.Title,
                        ["data"] = new JsonObject { ["id"] = link.Id },
                    },
                    ["items"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "TextBlock",
                            ["text"] = link.Title,
                            ["size"] = "ExtraLarge",
                            ["weight"] = "Bolder",
                            ["color"] = "Accent",
                            ["wrap"] = true,
                            ["horizontalAlignment"] = "Center",
                            ["spacing"] = "Large",
                        },
                    },
                });
            }
        }

        var card = new JsonObject
        {
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json",
            ["type"] = "AdaptiveCard",
            ["version"] = "1.5",
            ["body"] = body,
            ["actions"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "Action.Execute",
                    ["verb"] = "import",
                    ["title"] = "Import",
                },
                new JsonObject
                {
                    ["type"] = "Action.Execute",
                    ["verb"] = "export",
                    ["title"] = "Export",
                },
            },
        };

        return card.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }
}
