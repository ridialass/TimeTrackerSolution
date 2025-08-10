// TimeTracker.Mobile/Converters/NullToBoolConverter.cs
#nullable enable
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Converters
{
    /// <summary>
    /// Convertit null -> false, non-null -> true.
    /// Options :
    /// - <see cref="TreatWhitespaceAsNull"/> : considère les chaînes composées uniquement d'espaces comme null.
    /// - <see cref="Invert"/> ou ConverterParameter="invert" : inverse le résultat.
    /// </summary>
    public sealed class NullToBoolConverter : IValueConverter, IMarkupExtension
    {
        /// <summary>
        /// Si true, une chaîne blanche (ex: "  ") est traitée comme null. Par défaut: true.
        /// </summary>
        public bool TreatWhitespaceAsNull { get; set; } = true;

        /// <summary>Inverse le résultat (équivaut à passer ConverterParameter="invert").</summary>
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool notNull = value is not null;

            if (TreatWhitespaceAsNull && value is string s)
                notNull = !string.IsNullOrWhiteSpace(s);

            var invert = Invert || IsTrue(parameter);
            return invert ? !notNull : notNull; // toujours bool
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException($"{nameof(NullToBoolConverter)} does not support ConvertBack.");

        // Permet l’usage direct en XAML : {converters:NullToBoolConverter}
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

