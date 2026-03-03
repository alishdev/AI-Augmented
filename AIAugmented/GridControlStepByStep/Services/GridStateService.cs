namespace GridControlStepByStep.Services;

/// <summary>
/// Stores per-grid UI state (e.g. column widths) so it survives route navigation.
/// Registered as Scoped, which in Blazor WASM is effectively singleton for the session.
/// </summary>
public class GridStateService
{
    private readonly Dictionary<string, Dictionary<string, int>> _columnWidths = new();

    public Dictionary<string, int>? GetColumnWidths(string gridId) =>
        _columnWidths.TryGetValue(gridId, out var widths) ? widths : null;

    public void SaveColumnWidths(string gridId, Dictionary<string, int> widths) =>
        _columnWidths[gridId] = new Dictionary<string, int>(widths);
}
