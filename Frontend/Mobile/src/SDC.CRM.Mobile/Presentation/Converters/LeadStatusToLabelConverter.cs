using System.Globalization;
using SDC.CRM.Mobile.Presentation.Formatting;

namespace SDC.CRM.Mobile.Presentation.Converters;

/// <summary>
/// Shows the Polish label of a lead status code (e.g. "New" -> "Nowy").
/// The mapping lives in <see cref="LeadStatusLabels"/> (Core) where it is unit tested.
/// </summary>
public sealed class LeadStatusToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => LeadStatusLabels.For(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("Lead status labels are display-only.");
}

