using System;
using System.Globalization;
using System.Windows.Data;

namespace ResTB.GUI.Helpers.Converter
{
    /// <summary>
    /// true -> "Visible"
    /// <para/>
    /// false -> "Collapsed"
    /// </summary>
    class BoolVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// true -> "Collapsed"
    /// <para/>
    /// false -> "Visible"
    /// </summary>
    class BoolVisibiltyInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value != true ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
