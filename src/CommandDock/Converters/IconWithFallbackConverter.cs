using System;
using System.Globalization;
using System.Windows.Data;

namespace CommandDock.Converters;

public sealed class IconWithFallbackConverter : IValueConverter
{
    public const string DefaultIcon = "🔷";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var icon = value as string;
        return string.IsNullOrWhiteSpace(icon) ? DefaultIcon : icon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
