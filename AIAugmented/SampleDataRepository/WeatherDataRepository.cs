using FluentValidation;

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
/// Sort specification: column name and direction ("asc" or "desc").
/// </summary>
public record GridSortColumn(string Column, string Direction);

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
    /// Optional sort: column name and direction ("asc" or "desc"). When set, GetDataAsync returns sorted data.
    /// </summary>
    GridSortColumn? SortColumn { get; set; }
    /// <summary>
    /// Returns form field definitions for the given row, for use in row-action forms.
    /// actionName identifies the action (e.g. Edit, Delete) so details can vary by action.
    /// </summary>
    IReadOnlyList<RowActionField> RowActionDetails(string actionName, T row);
    /// <summary>
    /// Returns form field definitions for grid-level actions (e.g. Add). No row context.
    /// </summary>
    IReadOnlyList<RowActionField> GridActionDetails(string actionName);
    /// <summary>
    /// Invokes the row action (e.g. Edit, Delete). Parameters match RowActionDetails for consistency.
    /// </summary>
    Task InvokeAction(string actionName, T row);
    /// <summary>
    /// Returns the validator for the entity type, or null if no validation is configured.
    /// </summary>
    IValidator<T>? GetValidator { get; }
    Task<List<T>> GetDataAsync(int page, int pageSize);
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

    public GridSortColumn? SortColumn { get; set; }

    public IReadOnlyList<string> GetColumnNames() => ColumnNames;

    public GridCapabilities Capabilities => WeatherCapabilities;

    public IList<FilterColumn> FilterColumns => _filterColumns;

    public IValidator<WeatherData>? GetValidator => null;

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

    public IReadOnlyList<RowActionField> GridActionDetails(string actionName)
    {
        if (string.Equals(actionName, "Add", StringComparison.OrdinalIgnoreCase))
        {
            var summaryOptions = SummaryChoices.Select(s => (Id: s, Text: s)).ToList();
            return
            [
                new RowActionField(nameof(WeatherData.Date), DateTime.Now.ToString("yyyy-MM-dd"), [], "textbox"),
                new RowActionField(nameof(WeatherData.Temperature), "", [], "textbox"),
                new RowActionField(nameof(WeatherData.Summary), "", summaryOptions, "dropdown"),
            ];
        }
        return [];
    }

    public Task InvokeAction(string actionName, WeatherData row) => Task.CompletedTask;

    private List<WeatherData> _data;
    public WeatherDataRepository()
    {
        _data = Enumerable.Range(1, TotalRecords)
            .Select(i => new WeatherData {
                Date = DateTime.Now.AddDays(i),
                Temperature = Random.Shared.Next(-20, 55),
                Summary = SummaryChoices[Random.Shared.Next(SummaryChoices.Length)] })
            .ToList();
    }

    public async Task<List<WeatherData>> GetDataAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0");
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        if ((page-1) * pageSize > TotalRecords)
            return new List<WeatherData>();
        
        await Task.Delay(1000);

        var list = _data;        

        if (SortColumn is { } sort && GetProperty(typeof(WeatherData), sort.Column) is { } prop)
        {
            var desc = string.Equals(sort.Direction, "desc", StringComparison.OrdinalIgnoreCase);
            list = desc
                ? list.OrderByDescending(r => prop.GetValue(r)).ToList()
                : list.OrderBy(r => prop.GetValue(r)).ToList();
        }

        return list
            .Skip((page - 1) * pageSize)
            .Take(Math.Min(pageSize, TotalRecords - (page - 1) * pageSize))
            .ToList();
    }

    private static System.Reflection.PropertyInfo? GetProperty(Type type, string name)
    {
        return type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

public class WeatherData
{
    public DateTime Date { get; set; }
    public int Temperature { get; set; }
    public string Summary { get; set; } =  string.Empty;
}