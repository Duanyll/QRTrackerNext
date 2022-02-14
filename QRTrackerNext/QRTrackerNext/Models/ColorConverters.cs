﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace QRTrackerNext.Models
{
    static class LabelUtils
    {
        class ColorData
        {
            public string ChineseName { get; set; }
            public string AccentColorHex { get; set; }
            public string BackgroundColorHex { get; set; }
        }

        static Dictionary<string, ColorData> _colorData;
        static Dictionary<string, string> _chineseToName;

        static LabelUtils()
        {
            _colorData = new Dictionary<string, ColorData>()
            {
                ["noCheck"] = new ColorData()
                {
                    ChineseName = "未登记",
                    AccentColorHex = "#D3D3D3",
                    BackgroundColorHex = "#D3D3D3"
                },
                ["gray"] = new ColorData()
                {
                    ChineseName = "灰色",
                    AccentColorHex = "#778899",
                    BackgroundColorHex = "#D3D3D3"
                },
                ["red"] = new ColorData()
                {
                    ChineseName = "红色",
                    AccentColorHex = "#FF6558",
                    BackgroundColorHex = "#FF8A80",
                },
                ["yellow"] = new ColorData()
                {
                    ChineseName = "黄色",
                    AccentColorHex = "#FFD600",
                    BackgroundColorHex = "#FFD600",
                },
                ["green"] = new ColorData()
                {
                    ChineseName = "绿色",
                    AccentColorHex = "#00C853",
                    BackgroundColorHex = "#00E676"
                },
                ["blue"] = new ColorData()
                {
                    ChineseName = "蓝色",
                    AccentColorHex = "#5498F9",
                    BackgroundColorHex = "#90CAF9"
                },
                ["purple"] = new ColorData()
                {
                    ChineseName = "紫色",
                    AccentColorHex = "#D56BF0",
                    BackgroundColorHex = "#CE93D8"
                }
            };
            _chineseToName = new Dictionary<string, string>();
            foreach (var pair in _colorData)
            {
                _chineseToName[pair.Value.ChineseName] = pair.Key;
            }
        }

        public static Color NameToAccentXFColor(string colorName)
        {
            return Color.FromHex(_colorData[colorName].AccentColorHex);
        }

        public static SkiaSharp.SKColor NameToAccentSKColor(string colorName)
        {
            return SkiaSharp.SKColor.Parse(_colorData[colorName].AccentColorHex);
        }

        public static Color NameToBackgroundXFColor(string colorName)
        {
            return Color.FromHex(_colorData[colorName].BackgroundColorHex);
        }

        public static string NameToChineseDisplay(string colorName, HomeworkType type = null)
        {
            if ((type?.ColorDescriptions.TryGetValue(colorName, out var description) ?? false) && !string.IsNullOrEmpty(description))
            {
                return _colorData[colorName].ChineseName + ": " + description;
            }
            else
            {
                return _colorData[colorName].ChineseName;
            }
        }

        public static string ChineseDisplayToName(string chinese)
        {
            if (_chineseToName.TryGetValue(chinese.ToString().Split(':')[0], out var res))
            {
                return res;
            }
            else
            {
                return "gray";
            }
        }
    }

    class StringToAccentColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LabelUtils.NameToAccentXFColor(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class StringToBackgroundColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LabelUtils.NameToBackgroundXFColor(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class ColorChineseNameConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LabelUtils.NameToChineseDisplay(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LabelUtils.ChineseDisplayToName(value as string);
        }
    }
}
