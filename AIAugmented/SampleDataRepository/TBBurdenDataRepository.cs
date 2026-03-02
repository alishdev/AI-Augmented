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

    private static readonly GridCapabilities TbCapabilities = new(
        Filter: ColumnNames,
        GridActions: ["Add"],
        RowActions: ["Delete", "Edit"]
    );

    private readonly List<FilterColumn> _filterColumns = [];
    private static readonly Dictionary<string, PropertyInfo> PropertyMap = typeof(TBBurdenData)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    public TBBurdenDataRepository()
    {
        _data = new Lazy<List<TBBurdenData>>(LoadCsv);
    }

    public IReadOnlyList<string> GetColumnNames() => ColumnNames;

    public GridCapabilities Capabilities => TbCapabilities;

    public IList<FilterColumn> FilterColumns => _filterColumns;

    public async Task<List<TBBurdenData>> GetDataAsync(int page, int pageSize, int delayMS = 0)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0");
        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0");

        if (delayMS > 0)
            await Task.Delay(delayMS);

        var source = _data.Value;
        if (_filterColumns.Count > 0)
            source = ApplyFilters(source, _filterColumns).ToList();

        var skip = (page - 1) * pageSize;
        if (skip >= source.Count)
            return new List<TBBurdenData>();

        return source
            .Skip(skip)
            .Take(pageSize)
            .ToList();
    }

    private static IEnumerable<TBBurdenData> ApplyFilters(List<TBBurdenData> source, IList<FilterColumn> filterColumns)
    {
        foreach (var row in source)
        {
            var match = true;
            foreach (var f in filterColumns)
            {
                if (!PropertyMap.TryGetValue(f.Column, out var prop))
                    continue;
                var cellValue = prop.GetValue(row);
                var cellStr = cellValue?.ToString() ?? string.Empty;
                if (!Matches(cellValue, cellStr, f.Operator, f.Value))
                {
                    match = false;
                    break;
                }
            }
            if (match)
                yield return row;
        }
    }

    private static bool Matches(object? cellValue, string cellStr, string op, string filterValue)
    {
        var opUpper = op.Trim().ToUpperInvariant();
        filterValue = filterValue ?? string.Empty;

        return opUpper switch
        {
            "EQUAL" or "EQ" => cellStr.Equals(filterValue, StringComparison.OrdinalIgnoreCase)
                || TryCompareNumeric(cellValue, filterValue, out var eq) && eq == 0,
            "NOTEQUAL" or "NE" => !cellStr.Equals(filterValue, StringComparison.OrdinalIgnoreCase)
                && (!TryCompareNumeric(cellValue, filterValue, out var ne) || ne != 0),
            "CONTAINS" => cellStr.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
            "GREATERTHAN" or "GT" => TryCompareNumeric(cellValue, filterValue, out var gt) && gt > 0,
            "LESSTHAN" or "LT" => TryCompareNumeric(cellValue, filterValue, out var lt) && lt < 0,
            "GREATERTHANOREQUAL" or "GTE" => TryCompareNumeric(cellValue, filterValue, out var gte) && gte >= 0,
            "LESSTHANOREQUAL" or "LTE" => TryCompareNumeric(cellValue, filterValue, out var lte) && lte <= 0,
            _ => cellStr.Equals(filterValue, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool TryCompareNumeric(object? cellValue, string filterValue, out int comparison)
    {
        comparison = 0;
        if (string.IsNullOrWhiteSpace(filterValue)) return false;
        var filterVal = filterValue.Trim();
        if (cellValue is int i)
        {
            if (!int.TryParse(filterVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var f)) return false;
            comparison = i.CompareTo(f);
            return true;
        }
        if (cellValue is long l)
        {
            if (!long.TryParse(filterVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var f)) return false;
            comparison = l.CompareTo(f);
            return true;
        }
        if (cellValue is double d)
        {
            if (!double.TryParse(filterVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return false;
            comparison = d.CompareTo(f);
            return true;
        }
        if (cellValue is double dVal)
        {
            if (!double.TryParse(filterVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return false;
            comparison = dVal.CompareTo(f);
            return true;
        }
        return false;
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
