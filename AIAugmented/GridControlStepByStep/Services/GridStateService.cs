using SampleDataRepository;

namespace GridControlStepByStep.Services;

/// <summary>
/// Stores per-grid UI state (e.g. column widths, sort) so it survives route navigation.
/// Registered as Scoped, which in Blazor WASM is effectively singleton for the session.
/// </summary>
public class GridStateService
{
    private readonly Dictionary<string, Dictionary<string, int>> _columnWidths = new();
    private readonly Dictionary<string, GridSortColumn> _sortColumns = new();
    private readonly Dictionary<string, int> _pageSizes = new();

    public Dictionary<string, int>? GetColumnWidths(string gridId) =>
        _columnWidths.TryGetValue(gridId, out var widths) ? widths : null;

    public void SaveColumnWidths(string gridId, Dictionary<string, int> widths) =>
        _columnWidths[gridId] = new Dictionary<string, int>(widths);

    public GridSortColumn? GetSortColumn(string gridId) =>
        _sortColumns.TryGetValue(gridId, out var sort) ? sort : null;

    public void SaveSortColumn(string gridId, GridSortColumn? sort)
    {
        if (sort is not null)
            _sortColumns[gridId] = sort;
        else
            _sortColumns.Remove(gridId);
    }

    public int? GetPageSize(string gridId) =>
        _pageSizes.TryGetValue(gridId, out var size) ? size : null;

    public void SavePageSize(string gridId, int size) =>
        _pageSizes[gridId] = size;
}
