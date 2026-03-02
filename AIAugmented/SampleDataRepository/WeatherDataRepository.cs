namespace SampleDataRepository;

public interface IGridDataRepository<T> where T : class
{
    IReadOnlyList<string> GetColumnNames();
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

    public IReadOnlyList<string> GetColumnNames() => ColumnNames;

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