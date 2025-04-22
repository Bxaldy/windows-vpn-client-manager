using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BuiKuVPN.Classes
{
    public class OnlineStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString().ToLower() == "offline")
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.ForestGreen;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
