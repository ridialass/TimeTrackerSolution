// TimeTracker.Mobile/Converters/InverseBoolConverter.cs
#nullable enable
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Converters
{
    /// <summary>
    /// Inverse un booléen (true -> false, false -> true).
    /// - Supporte bool et bool?.
    /// - Si la valeur n'est pas un bool/nullable bool, retourne false (fallback sûr pour propriétés bool non-nullables).
    /// - Utilisable directement en XAML grâce à IMarkupExtension.
    /// </summary>
    public sealed class InverseBoolConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : !(value as bool? ?? false);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : !(value as bool? ?? false);

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => this;
    }

}
