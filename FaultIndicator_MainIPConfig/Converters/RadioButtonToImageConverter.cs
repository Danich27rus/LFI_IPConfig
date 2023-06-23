using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FaultIndicator_MainIPConfig.Converters
{
    internal class RadioButtonToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int integer = (int)value;
            if (!string.IsNullOrEmpty((string?)parameter))
            {
                switch (integer)
                {
                    case 1:
                        return new BitmapImage(new Uri("/Images/LFI_all.png", UriKind.Relative));
                    case 2:
                        return new BitmapImage(new Uri("/Images/LFI_BB.png", UriKind.Relative));
                    case 3:
                        return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return parameter;
            return Binding.DoNothing;
        }
    }
}
