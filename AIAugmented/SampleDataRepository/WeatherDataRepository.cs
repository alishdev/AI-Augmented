namespace SampleDataRepository;

/// <summary>
/// Describes which capabilities the grid supports: Filter (column names), GridActions, RowActions.
/// Each property returns an array of capability names.
/// </summary>
public record GridCapabilities(
    IReadOnlyList<string> Filter,
    IReadOnlyList<string> GridActions,
    IReadOnlyList<string> RowActions
);

/// <summary>
/// A single filter condition, e.g. {"Column":"Date","Operator":"Equal","Value":"2026-03-02"}.
/// </summary>
public record FilterColumn(string Column, string Operator, string Value);

/// <summary>
/// Describes a single field to display in a row-action form.
/// PossibleValues are (Id, Text) pairs for dropdown options.
/// Visual is the control type: label, textbox, dropdown, etc.
/// </summary>
public record RowActionField(
    string Name,
    string Value,
    IReadOnlyList<(string Id, string Text)> PossibleValues,
    string Visual
);

public interface IGridDataRepository<T> where T : class
{
    IReadOnlyList<string> GetColumnNames();
    GridCapabilities Capabilities { get; }
    /// <summary>
    /// Active filter conditions. When not empty, repositories may filter records (e.g. TBBurdenDataRepository).
    /// </summary>
    IList<FilterColumn> FilterColumns { get; }
    /// <summary>
    /// Returns form field definitions for the given row, for use in row-action forms.
    /// actionName identifies the action (e.g. Edit, Delete) so details can vary by action.
    /// </summary>
    IReadOnlyList<RowActionField> RowActionDetails(string actionName, T row);
    /// <summary>
    /// Invokes the row action (e.g. Edit, Delete). Parameters match RowActionDetails for consistency.
    /// </summary>
    Task InvokeAction(string actionName, T row);
    Task<List<T>> GetDataAsync(int page, int pageSize, int delayMS = 0);
}

public class WeatherDataRepository : IGridDataRepository<WeatherData>
{
    const int TotalRecords = 23;

    static readonly string[] ColumnNames = [nameof(WeatherData.Date), nameof(WeatherData.Temperature), nameof(WeatherData.Summary)];

    static readonly string[] SummaryChoices =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    private static readonly GridCapabilities WeatherCapabilities = new(
        Filter: [],
        GridActions: ["Add"],
        RowActions: ["Delete"]
    );

    private readonly List<FilterColumn> _filterColumns = [];

    public IReadOnlyList<string> GetColumnNames() => ColumnNames;

    public GridCapabilities Capabilities => WeatherCapabilities;

    public IList<FilterColumn> FilterColumns => _filterColumns;

    public IReadOnlyList<RowActionField> RowActionDetails(string actionName, WeatherData row)
    {
        var summaryOptions = SummaryChoices.Select(s => (Id: s, Text: s)).ToList();
        return
        [
            new RowActionField(nameof(WeatherData.Date), row.Date.ToString("yyyy-MM-dd"), [], "label"),
            new RowActionField(nameof(WeatherData.Temperature), row.Temperature.ToString(), [], "textbox"),
            new RowActionField(nameof(WeatherData.Summary), row.Summary, summaryOptions, "dropdown"),
        ];
    }

    public Task InvokeAction(string actionName, WeatherData row) => Task.CompletedTask;

    public async Task<List<WeatherData>> GetDataAsync(int page, int pageSize, int delayMS = 0)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0");
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        if ((page-1) * pageSize > TotalRecords)
            return new List<WeatherData>();
        
        if (delayMS > 0)
            await Task.Delay(delayMS);

        return Enumerable.Range(1, TotalRecords)
            .Skip((page - 1) * pageSize)
            .Take(Math.Min(pageSize, TotalRecords - (page - 1) * pageSize))
            .Select(i => new WeatherData { 
                Date = DateTime.Now.AddDays(i), 
                Temperature = Random.Shared.Next(-20, 55), 
                Summary = SummaryChoices[Random.Shared.Next(SummaryChoices.Length)] })
            .ToList();
    }
}

public class WeatherData
{
    public DateTime Date { get; set; }
    public int Temperature { get; set; }
    public string Summary { get; set; } =  string.Empty;
}