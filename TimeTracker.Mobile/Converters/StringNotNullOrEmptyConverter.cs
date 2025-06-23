﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Converters
{
    public class StringNotNullOrEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value?.ToString());

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
