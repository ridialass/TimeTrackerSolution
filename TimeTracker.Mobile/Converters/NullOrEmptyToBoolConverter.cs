// TimeTracker.Mobile/Converters/NullOrEmptyToBoolConverter.cs
#nullable enable
using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Converters
{
    /// <summary>
    /// Retourne true si la valeur n'est pas nulle/vides :
    /// - string : non null, et non vide (ou non white-space selon <see cref="TreatWhitespaceAsEmpty"/>)
    /// - ICollection/IEnumerable : contient au moins 1 élément
    /// - Autres types : true si non null
    /// <para/>Options :
    /// - <see cref="Invert"/> : inverse le résultat
    /// - Paramètre "invert" ou "true" pour inverser ponctuellement
    /// </summary>
    public sealed class NullOrEmptyToBoolConverter : IValueConverter, IMarkupExtension
    {
        /// <summary>Considérer les chaînes de blancs comme vides. Par défaut: true.</summary>
        public bool TreatWhitespaceAsEmpty { get; set; } = true;

        /// <summary>Inverse le résultat (équivalent à passer ConverterParameter="invert").</summary>
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool hasValue;

            if (value is string s)
            {
                hasValue = TreatWhitespaceAsEmpty ? !string.IsNullOrWhiteSpace(s)
                                                  : s.Length > 0;
            }
            else if (value is ICollection col)
            {
                hasValue = col.Count > 0;
            }
            else if (value is IEnumerable en)
            {
                // Détermine s'il y a au moins un élément sans allouer
                var e = en.GetEnumerator();
                hasValue = e.MoveNext();
                (e as IDisposable)?.Dispose();
            }
            else
            {
                hasValue = value is not null;
            }

            var invert = Invert || IsTrue(parameter);
            return invert ? !hasValue : hasValue; // toujours bool non-nullable
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException($"{nameof(NullOrEmptyToBoolConverter)} does not support ConvertBack.");

        // Permet l’usage direct en XAML : {converters:NullOrEmptyToBoolConverter}
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
