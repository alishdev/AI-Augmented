namespace GridControlComposer.Services;

/// <summary>
/// Persists grid state (e.g. VisibleColumns) across navigation.
/// Scoped service survives route changes within the app session.
/// </summary>
public class GridStateService
{
    private readonly Dictionary<string, List<string>> _visibleColumnsByKey = new();

    public IReadOnlyList<string>? GetVisibleColumns(string key)
    {
        return _visibleColumnsByKey.TryGetValue(key, out var cols) ? cols : null;
    }

    public void SetVisibleColumns(string key, List<string> columns)
    {
        _visibleColumnsByKey[key] = new List<string>(columns);
    }
}
