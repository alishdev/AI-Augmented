namespace SampleDataRepository;

public class WeatherDataRepository
{
    const int TotalRecords = 23;

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

    public static async Task<List<WeatherData>> GetWeatherDataAsync(int page, int pageSize, int delayMS = 0)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0");
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        if (page * pageSize > TotalRecords)
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