namespace Links;

internal sealed record LinkItem(
    string Id,
    string Type,
    string Title,
    string Target,
    string? WorkingDirectory,
    bool KeepWindowOpen)
{
    internal const string TypeRdp = "rdp";
    internal const string TypeBatch = "batch";
}
