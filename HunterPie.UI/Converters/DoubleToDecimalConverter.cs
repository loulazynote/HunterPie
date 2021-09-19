﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace HunterPie.UI.Converters
{
    public class DoubleToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            return (int)((val % 1) * 100);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
