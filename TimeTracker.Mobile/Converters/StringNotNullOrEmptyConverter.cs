// TimeTracker.Mobile/Converters/StringNotNullOrEmptyConverter.cs
#nullable enable
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Converters
{
    /// <summary>
    /// true si la valeur (string ou autre) n'est pas nulle/vides.
    /// - string : vérifie vide/whitespace selon <see cref="TreatWhitespaceAsEmpty"/>
    /// - autres types : utilise ToString(); null => false
    /// Options :
    /// - <see cref="TreatWhitespaceAsEmpty"/> : par défaut true
    /// - <see cref="Invert"/> ou ConverterParameter="invert" : inverse le résultat
    /// </summary>
    public sealed class StringNotNullOrEmptyConverter : IValueConverter, IMarkupExtension
    {
        /// <summary>
        /// Si true, les chaînes composées uniquement d'espaces sont considérées vides.
        /// </summary>
        public bool TreatWhitespaceAsEmpty { get; set; } = true;

        /// <summary>
        /// Inverse le résultat (équivaut à passer ConverterParameter="invert").
        /// </summary>
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool hasText;

            if (value is string s)
            {
                hasText = TreatWhitespaceAsEmpty ? !string.IsNullOrWhiteSpace(s)
                                                 : s.Length > 0;
            }
            else if (value is null)
            {
                hasText = false;
            }
            else
            {
                var str = value.ToString() ?? string.Empty;
                hasText = TreatWhitespaceAsEmpty ? !string.IsNullOrWhiteSpace(str)
                                                 : str.Length > 0;
            }

            var invert = Invert || IsTrue(parameter);
            return invert ? !hasText : hasText; // toujours bool (non-nullable)
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException($"{nameof(StringNotNullOrEmptyConverter)} does not support ConvertBack.");

        // Permet l’usage direct en XAML : {converters:StringNotNullOrEmptyConverter}
        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => this;

        private static bool IsTrue(object? parameter)
        {
            if (parameter is null) return false;
            if (parameter is bool b) return b;
            if (parameter is string s)
            {
                if (bool.TryParse(s, out var parsed)) return parsed;
                return string.Equals(s, "invert", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
