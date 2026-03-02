using System.Globalization;
using System.Reflection;
using System.Text;

namespace SampleDataRepository;

public class TBBurdenData
{
    public string CountryName { get; set; } = string.Empty;
    public string Iso3Code { get; set; } = string.Empty;
    public int IsoNumericCode { get; set; }
    public string Region { get; set; } = string.Empty;
    public int Year { get; set; }
    public long Population { get; set; }
    public int PrevalencePer100k { get; set; }
    public int PrevalencePer100kLow { get; set; }
    public string MortalityEstimateMethod { get; set; } = string.Empty;
    public double? HivInIncidentTbPercent { get; set; }
    public double? CaseDetectionRatePercent { get; set; }
}

public class TBBurdenDataRepository : IGridDataRepository<TBBurdenData>
{
    private const string CsvResourceName = "SampleDataRepository.TB_Burden_Country.csv";
    private static readonly string[] ColumnNames =
    [
        nameof(TBBurdenData.CountryName),
        nameof(TBBurdenData.Iso3Code),
        nameof(TBBurdenData.IsoNumericCode),
        nameof(TBBurdenData.Region),
        nameof(TBBurdenData.Year),
        nameof(TBBurdenData.Population),
        nameof(TBBurdenData.PrevalencePer100k),
        nameof(TBBurdenData.PrevalencePer100kLow),
        nameof(TBBurdenData.MortalityEstimateMethod),
        nameof(TBBurdenData.HivInIncidentTbPercent),
        nameof(TBBurdenData.CaseDetectionRatePercent),
    ];

    private readonly Lazy<List<TBBurdenData>> _data;

    public TBBurdenDataRepository()
    {
        _data = new Lazy<List<TBBurdenData>>(LoadCsv);
    }

    public IReadOnlyList<string> GetColumnNames() => ColumnNames;

    public async Task<List<TBBurdenData>> GetDataAsync(int page, int pageSize, int delayMS = 0)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0");
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        if (delayMS > 0)
            await Task.Delay(delayMS);

        var all = _data.Value;
        var skip = (page - 1) * pageSize;
        if (skip >= all.Count)
            return new List<TBBurdenData>();

        return all
            .Skip(skip)
            .Take(pageSize)
            .ToList();
    }

    private static List<TBBurdenData> LoadCsv()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(CsvResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{CsvResourceName}' not found.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
            lines.Add(line);

        if (lines.Count < 2)
            return new List<TBBurdenData>();

        // Skip header
        var result = new List<TBBurdenData>(lines.Count - 1);
        for (var i = 1; i < lines.Count; i++)
        {
            var row = ParseCsvLine(lines[i]);
            if (row.Count < 11)
                continue;

            var record = new TBBurdenData
            {
                CountryName = row[0].Trim(),
                Iso3Code = row[1].Trim(),
                IsoNumericCode = ParseInt(row[2]),
                Region = row[3].Trim(),
                Year = ParseInt(row[4]),
                Population = ParseLong(row[5]),
                PrevalencePer100k = ParseInt(row[6]),
                PrevalencePer100kLow = ParseInt(row[7]),
                MortalityEstimateMethod = row[8].Trim(),
                HivInIncidentTbPercent = ParseDoubleOrNull(row[9]),
                CaseDetectionRatePercent = ParseDoubleOrNull(row[10]),
            };
            result.Add(record);
        }

        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static int ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static long ParseLong(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        return long.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static double? ParseDoubleOrNull(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
}
