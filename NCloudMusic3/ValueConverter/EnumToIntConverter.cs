using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.ValueConverter
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, value.ToString());
        }
    }
}
