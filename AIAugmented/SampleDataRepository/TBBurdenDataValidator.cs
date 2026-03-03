using FluentValidation;

namespace SampleDataRepository;

public class TBBurdenDataValidator : AbstractValidator<TBBurdenData>
{
    public TBBurdenDataValidator()
    {
        // String fields
        RuleFor(x => x.CountryName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Iso3Code).NotEmpty().Length(3);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(10);
        RuleFor(x => x.MortalityEstimateMethod).MaximumLength(100);

        // Integer/long fields: non-negative where applicable
        RuleFor(x => x.IsoNumericCode).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.Now.Year)
            .WithMessage("Year must be between 1900 and {MaxValue}.");
        RuleFor(x => x.Population).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PrevalencePer100k).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PrevalencePer100kLow).GreaterThanOrEqualTo(0);

        // Nullable decimals with at most 2 decimal places
        RuleFor(x => x.HivInIncidentTbPercent)
            .Must(v => !v.HasValue || HasAtMostTwoDecimalPlaces(v.Value))
            .WithMessage("'{PropertyName}' must have at most 2 decimal places.");
        RuleFor(x => x.CaseDetectionRatePercent)
            .Must(v => !v.HasValue || HasAtMostTwoDecimalPlaces(v.Value))
            .WithMessage("'{PropertyName}' must have at most 2 decimal places.");
    }

    private static bool HasAtMostTwoDecimalPlaces(double value) =>
        Math.Round(value, 2) == value;
}
