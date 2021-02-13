using System;
using System.Globalization;
using System.Windows.Data;

namespace ResTB.GUI.Helpers.Converter
{
    /// <summary>
    /// string != null | White: "Visible"
    /// <para/>
    /// string == null | White: "Collapsed"
    /// </summary>
    class StringVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrWhiteSpace((string)value) ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// string != null | White: "Collapsed"
    /// <para/>
    /// string == null | White: "Visible"
    /// </summary>
    class StringVisibiltyInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrWhiteSpace((string)value) ? "Visible" : "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
