using System.Xml.Linq;

namespace Links;

internal static class LinkStore
{
    internal static string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Links");

    internal static string FilePath { get; } = Path.Combine(DirectoryPath, "links.xml");

    internal static IReadOnlyList<LinkItem> Load()
    {
        EnsureDefaultFile();
        return Read(FilePath);
    }

    internal static bool TryImport(string path)
    {
        try
        {
            var links = Read(path);
            Write(FilePath, links);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static void Export(string path)
    {
        EnsureDefaultFile();
        var destinationDirectory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Copy(FilePath, path, overwrite: true);
    }

    internal static IReadOnlyList<LinkItem> DefaultLinks { get; } =
    [
        new LinkItem("rdp-lab", LinkItem.TypeRdp, "RDP to 10.0.0.1", "10.0.0.1", null, false),
        new LinkItem(
            "db",
            LinkItem.TypeBatch,
            "Build DB",
            @"X:\workspace\build_db.bat",
            @"X:\workspace",
            true),
    ];

    private static void EnsureDefaultFile()
    {
        Directory.CreateDirectory(DirectoryPath);
        if (!File.Exists(FilePath))
        {
            Write(FilePath, DefaultLinks);
        }
    }

    private static IReadOnlyList<LinkItem> Read(string path)
    {
        var document = XDocument.Load(path);
        var links = new List<LinkItem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var root = document.Root ?? throw new InvalidDataException("The links XML file is missing a root element.");

        foreach (var element in root.Elements("link"))
        {
            var id = (string?)element.Attribute("id");
            var type = (string?)element.Attribute("type");
            var title = (string?)element.Attribute("title");
            var target = (string?)element.Attribute("target");
            var workingDirectory = (string?)element.Attribute("workingDirectory");
            var keepWindowOpen = (bool?)element.Attribute("keepWindowOpen") ?? false;

            if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(type)
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(target)
                || !seen.Add(id)
                || !IsKnownType(type))
            {
                continue;
            }

            links.Add(new LinkItem(id, type, title, target, workingDirectory, keepWindowOpen));
        }

        return links;
    }

    private static void Write(string path, IReadOnlyList<LinkItem> links)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? DirectoryPath);

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(
                "links",
                links.Select(static link =>
                {
                    var element = new XElement(
                        "link",
                        new XAttribute("id", link.Id),
                        new XAttribute("type", link.Type),
                        new XAttribute("title", link.Title),
                        new XAttribute("target", link.Target));

                    if (!string.IsNullOrWhiteSpace(link.WorkingDirectory))
                    {
                        element.Add(new XAttribute("workingDirectory", link.WorkingDirectory));
                    }

                    if (link.KeepWindowOpen)
                    {
                        element.Add(new XAttribute("keepWindowOpen", true));
                    }

                    return element;
                })));

        document.Save(path);
    }

    private static bool IsKnownType(string type) =>
        type.Equals(LinkItem.TypeRdp, StringComparison.OrdinalIgnoreCase)
        || type.Equals(LinkItem.TypeBatch, StringComparison.OrdinalIgnoreCase);
}
