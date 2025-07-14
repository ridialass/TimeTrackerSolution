using Microsoft.Extensions.Localization;
using System;

namespace TimeTracker.Core.Helpers
{
    public static class EnumLocalizationHelper
    {
        /// <summary>
        /// Retourne le label localisé pour une valeur d'enum (non nullable).
        /// </summary>
        public static string GetEnumLabel<TEnum>(TEnum value, IStringLocalizer localizer)
            where TEnum : Enum
        {
            string key = $"{typeof(TEnum).Name}_{value}";
            var localized = localizer[key];
            if (localized.ResourceNotFound)
                return value.ToString();
            return localized.Value;
        }

        /// <summary>
        /// Retourne le label localisé pour une valeur d'enum nullable.
        /// Si la valeur est nulle, retourne le label par défaut (ex : "Non défini").
        /// </summary>
        public static string GetEnumLabel<TEnum>(TEnum? value, IStringLocalizer localizer, string fallback = "Non défini")
            where TEnum : struct, Enum
        {
            if (value.HasValue)
                return GetEnumLabel(value.Value, localizer);
            return fallback;
        }
    }
}