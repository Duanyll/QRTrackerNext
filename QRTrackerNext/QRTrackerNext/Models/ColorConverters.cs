using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace QRTrackerNext.Models
{
    class StringToAccentColorConvertor : IValueConverter
    {
        static Dictionary<string, Color> strToCol = new Dictionary<string, Color>()
        {
            { "red", Color.FromHex("#FF6558") },
            { "yellow", Color.FromHex("#FDD835") },
            { "green", Color.FromHex("#00C853") },
            { "blue", Color.FromHex("#5498F9") },
            { "purple", Color.FromHex("#D56BF0") },
            { "grey", Color.LightGray }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (strToCol.TryGetValue(value.ToString(), out var res))
            {
                return res;
            }
            else
            {
                return Color.LightGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class StringToBackgroundColorConvertor : IValueConverter
    {
        static Dictionary<string, Color> strToCol = new Dictionary<string, Color>()
        {
            { "red", Color.FromHex("#FF8A80") },
            { "yellow", Color.FromHex("#FFF59D") },
            { "green", Color.FromHex("#B0FF57") },
            { "blue", Color.FromHex("#90CAF9") },
            { "purple", Color.FromHex("#CE93D8") },
            { "grey", Color.WhiteSmoke }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (strToCol.TryGetValue(value.ToString(), out var res))
            {
                return res;
            }
            else
            {
                return Color.WhiteSmoke;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class ColorChineseNameConvertor : IValueConverter
    {
        static Dictionary<string, string> strToCol = new Dictionary<string, string>()
        {
            { "red", "红色" },
            { "yellow", "黄色" },
            { "green", "绿色" },
            { "blue", "蓝色" },
            { "purple", "紫色" },
            { "grey", "灰色" }
        };

        static Dictionary<string, string> back = new Dictionary<string, string>()
        {
            { "红色", "red" },
            { "黄色", "yellow" },
            { "绿色", "green" },
            { "蓝色", "blue" },
            { "紫色", "purple" },
            { "灰色", "grey" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (strToCol.TryGetValue(value.ToString(), out var res))
            {
                return res;
            }
            else
            {
                return "灰色";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (back.TryGetValue(value.ToString(), out var res))
            {
                return res;
            }
            else
            {
                return "grey";
            }
        }
    }
}
