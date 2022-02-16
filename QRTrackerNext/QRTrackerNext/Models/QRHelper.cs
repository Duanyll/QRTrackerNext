using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using Realms;

using MongoDB.Bson;
using Xamarin.Essentials;

using SkiaSharp;
using ZXing;

namespace QRTrackerNext.Models
{
    static class QRHelper
    {
        public static string GetStudentUri(Student student)
        {
            return $"https://qrt.duanyll.com/stu?id={student.Id}";
        }

        public static ObjectId ParseStudentUri(string str)
        {
            if (str.StartsWith("https://qrt.duanyll.com/stu") || str.StartsWith("qrt://stu"))
            {
                return ObjectId.Parse(str.Split('=')[1]);
            }
            else if (ObjectId.TryParse(str, out ObjectId id))
            {
                return id;
            }
            else
            {
                return ObjectId.Empty;
            }
        }

        public static string GetStudentUriShort(Student student)
        {
            return $"qrt://stu?id={student.Id}";
        }

        const int QRCODE_WIDTH = 200;
        const int QRCODE_HEIGHT = 200;
        const int QRCODE_TEXT_SIZE = 48;
        const int QRCODE_PADDING = 40;

        public static List<SKBitmap> GetClassQrCodePic(IList<Student> stuList, int w, int h)
        {
            int cur = 0;
            List<SKBitmap> res = new List<SKBitmap>();
            if (w <= 0 || h <= 0) return res;
            using (SKPaint textPaint = new SKPaint() { TextSize = QRCODE_TEXT_SIZE, Typeface = SKFontManager.Default.MatchCharacter('汉') })
            {
                SKRect bounds = new SKRect();
                textPaint.MeasureText("测试文字", ref bounds);
                int bitmapWidth = QRCODE_PADDING * (w + 1) + QRCODE_WIDTH * w;
                int bitmapHeight = QRCODE_PADDING * (h + 1) + (QRCODE_HEIGHT + (int)bounds.Height) * h;

                var writer = new BarcodeWriter<SKBitmap>()
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions()
                    {
                        Height = QRCODE_HEIGHT,
                        Width = QRCODE_WIDTH,
                        Margin = 0
                    },
                    Renderer = new Utils.SKBitmapRenderer()
                };
                while (cur < stuList.Count)
                {
                    SKBitmap bitmap = new SKBitmap(bitmapWidth, bitmapHeight);
                    bitmap.Erase(new SKColor(0xFF, 0xFF, 0xFF));
                    using (SKCanvas canvas = new SKCanvas(bitmap))
                    {
                        for (int i = 0; i < h; i++)
                        {
                            for (int j = 0; j < w; j++)
                            {
                                if (cur >= stuList.Count)
                                {
                                    goto END;
                                }
                                var student = stuList[cur++];
                                var qrBitmap = writer.Write($"https://qrt.duanyll.com/stu?id={student.Id}");

                                var offsetTop = QRCODE_PADDING * (i + 1) + (QRCODE_HEIGHT + (int)bounds.Height) * i;
                                var offsetLeft = QRCODE_PADDING * (j + 1) + QRCODE_WIDTH * j;
                                canvas.DrawBitmap(qrBitmap, new SKPoint() { X = offsetLeft, Y = offsetTop });
                                canvas.DrawText(student.Name, new SKPoint() { X = offsetLeft, Y = offsetTop + QRCODE_HEIGHT + (int)bounds.Height }, textPaint);
                            }
                        }
                    }
                END:
                    res.Add(bitmap);
                }
            }

            return res;
        }

        const int PDF417_WIDTH = 300;
        const int PDF417_HEIGHT = 100;
        const int PDF417_TEXT_SIZE = 36;
        const int PDF417_PADDING = 40;

        public static List<SKBitmap> GetClassPDF417CodePic(IList<Student> stuList, int w, int h)
        {
            int cur = 0;
            List<SKBitmap> res = new List<SKBitmap>();
            if (w <= 0 || h <= 0) return res;
            using (SKPaint textPaint = new SKPaint() { TextSize = PDF417_TEXT_SIZE, Typeface = SKFontManager.Default.MatchCharacter('汉') })
            {
                SKRect bounds = new SKRect();
                textPaint.MeasureText("测试文字", ref bounds);
                int bitmapWidth = PDF417_PADDING * (w + 1) + PDF417_WIDTH * w;
                int bitmapHeight = PDF417_PADDING * (h + 1) + (PDF417_HEIGHT + (int)bounds.Height) * h;

                var writer = new BarcodeWriter<SKBitmap>()
                {
                    Format = BarcodeFormat.PDF_417,
                    Options = new ZXing.Common.EncodingOptions()
                    {
                        Height = PDF417_HEIGHT,
                        Width = PDF417_WIDTH,
                        Margin = 0
                    },
                    Renderer = new Utils.SKBitmapRenderer()
                };
                while (cur < stuList.Count)
                {
                    SKBitmap bitmap = new SKBitmap(bitmapWidth, bitmapHeight);
                    bitmap.Erase(new SKColor(0xFF, 0xFF, 0xFF));
                    using (SKCanvas canvas = new SKCanvas(bitmap))
                    {
                        for (int i = 0; i < h; i++)
                        {
                            for (int j = 0; j < w; j++)
                            {
                                if (cur >= stuList.Count)
                                {
                                    goto END;
                                }
                                try
                                {
                                    var student = stuList[cur++];
                                    var qrBitmap = writer.Write($"qrt://stu?id={student.Id}");

                                    var offsetTop = PDF417_PADDING * (i + 1) + (PDF417_HEIGHT + (int)bounds.Height) * i;
                                    var offsetLeft = PDF417_PADDING * (j + 1) + PDF417_WIDTH * j;
                                    canvas.DrawBitmap(qrBitmap, new SKPoint() { X = offsetLeft, Y = offsetTop });
                                    canvas.DrawText(student.Name, new SKPoint() { X = offsetLeft, Y = offsetTop + PDF417_HEIGHT + (int)bounds.Height }, textPaint);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                END:
                    res.Add(bitmap);
                }
            }

            return res;
        }

        public static string ExportStatsCSV(ObjectId groupId, ObjectId[] homeworksId)
        {
            var colorNames = new Dictionary<string, string>()
            {
                { "red", "红" },
                { "yellow", "黄" },
                { "green", "绿" },
                { "blue", "蓝" },
                { "purple", "紫" },
                { "gray", "√" }
            };

            var realm = Services.RealmManager.OpenDefault();
            var group = realm.Find<Group>(groupId);
            var homeworks = homeworksId.Select(i => realm.Find<Homework>(i)).ToArray();
            var stateMap = homeworks.Select(cur =>
            {
                var map = new Dictionary<ObjectId, string>();
                foreach (var i in cur.Status)
                {
                    var type = cur.Type;
                    string text;
                    if (!i.HasScanned)
                    {
                        text = type.NotCheckedDescription ?? "";
                    }
                    else if (type.Colors.Count == 0)
                    {
                        text = "√";
                    }
                    else if (i.Color == "gray")
                    {
                        text = string.IsNullOrEmpty(type.NoColorDescription) ? "√" : type.NoColorDescription;
                    }
                    else
                    {
                        if (!type.ColorDescriptions.TryGetValue(i.Color, out text) || string.IsNullOrEmpty(text))
                        {
                            text = colorNames[i.Color];
                        }
                    }
                    map.Add(i.Student.Id, text);
                }
                return map;
            });

            var res = new StringBuilder("姓名,");
            foreach (var i in homeworks)
            {
                res.Append(i.Name);
                res.Append(',');
            }
            res.AppendLine();
            foreach (var i in group.Students)
            {
                res.Append(i.Name);
                res.Append(',');
                foreach (var map in stateMap)
                {
                    if (map.TryGetValue(i.Id, out var state))
                    {
                        res.Append(state);
                    }
                    res.Append(',');
                }
                res.AppendLine();
            }

            return res.ToString();
        }
    }
}
