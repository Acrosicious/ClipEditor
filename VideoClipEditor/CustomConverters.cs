using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VideoClipEditor
{
    class VideoAspectToBoundingBox : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double d = (Double)value;
            if (d == 0)
                return new Thickness(-99999, 0, 0, 0);

            d *= -1.0;
            d += 50; // Needs Multiconverter for different Buttons
            return new Thickness(d,5,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
