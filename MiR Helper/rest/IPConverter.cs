using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace MiR_Helper.rest
{
    [ValueConversion(typeof(string), typeof(IPAddress))]
    public class IPConverter : IValueConverter
    {
        internal static readonly IValueConverter Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ipAsString = (string)value;
            return IPAddress.Parse(ipAsString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ipAsIPAddress = (IPAddress)value;
            string ipAddressAsString = ipAsIPAddress.ToString();
/*            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }*/
            return ipAddressAsString;
        }
    }
}
