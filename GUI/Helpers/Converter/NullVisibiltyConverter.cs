using System;
using System.Globalization;
using System.Windows.Data;

namespace ResTB.GUI.Helpers.Converter
{
    /// <summary>
    /// object != null: "Visible"
    /// <para/>
    /// object == null: "Hidden"
    /// </summary>
    class NullVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "Hidden" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// object != null: "Visible"
    /// <para/>
    /// object == null: "Collapsed"
    /// </summary>
    class NullVisibiltyCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// object != null: "Collapsed"
    /// <para/>
    /// object == null: "Visible"
    /// </summary>
    class NullVisibiltyInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
