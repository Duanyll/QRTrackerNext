using System;
using System.Collections.Generic;
using System.Text;

using Microcharts;
using System.Linq;

namespace QRTrackerNext.Models
{
    static internal class ChartUtils
    {
        public static IEnumerable<ChartEntry> GetHomeworkStatusPieChartEntries(IQueryable<HomeworkStatus> status, HomeworkType type)
        {
            var submittedStatus = status.Where(i => i.HasScanned);
            int notSubmittedCount = status.Count() - submittedStatus.Count();
            if (type.Colors.Count == 0)
            {
                return new ChartEntry[]
                {
                    new ChartEntry(submittedStatus.Count())
                    {
                        Label = string.IsNullOrEmpty(type.NotCheckedDescription) ? "已登记" : type.NotCheckedDescription,
                        ValueLabel = "green",
                        Color = LabelUtils.NameToAccentSKColor("green")
                    },
                    new ChartEntry(notSubmittedCount)
                    {
                        Label = string.IsNullOrEmpty(type.NoColorDescription) ? "未登记" : type.NoColorDescription,
                        ValueLabel = "noCheck",
                        Color = LabelUtils.NameToAccentSKColor("noCheck")
                    }
                };
            }
            else
            {
                var colorCount = new Dictionary<string, int>()
                {
                    {"gray", 0 },
                    {"green" , 0 },
                    {"yellow", 0 },
                    {"red", 0 },
                    {"blue", 0 },
                    {"purple", 0 }
                };
                foreach (var s in submittedStatus)
                {
                    colorCount[s.Color]++;
                }
                var res = new List<ChartEntry>()
                {
                    new ChartEntry(notSubmittedCount)
                    {
                        Label = string.IsNullOrEmpty(type.NotCheckedDescription) ? "未登记" : type.NotCheckedDescription,
                        ValueLabel = "noCheck",
                        Color = LabelUtils.NameToAccentSKColor("noCheck")
                    }
                };
                if (colorCount["gray"] > 0)
                {
                    res.Add(new ChartEntry(colorCount["gray"])
                    {
                        Label = string.IsNullOrEmpty(type.NoColorDescription) ? "未标记颜色" : type.NoColorDescription,
                        ValueLabel = "gray",
                        Color = LabelUtils.NameToAccentSKColor("gray")
                    });
                }
                foreach (var color in LabelUtils.allColors)
                {
                    if (colorCount[color] > 0 || type.Colors.Contains(color))
                    {
                        res.Add(new ChartEntry(colorCount[color])
                        {
                            Label = LabelUtils.NameToChineseDisplay(color, type),
                            ValueLabel = color,
                            Color = LabelUtils.NameToAccentSKColor(color)
                        });
                    }
                }
                return res;
            }
        }
    }
}
