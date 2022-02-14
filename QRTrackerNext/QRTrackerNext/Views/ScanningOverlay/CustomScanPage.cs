using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using System.IO;
using System.Reflection;
using Acr.UserDialogs;
using QRTrackerNext.Models;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class CustomScanPage : ContentPage
    {
        private ZXingScannerView zxing;
        private Overlay overlay;

        private Label label;
        private Frame frame;

        private StackLayout colorButtons;
        private Action<string> colorCallback;
        private string defaultColor;

        Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;

            var stream = assembly.GetManifestResourceStream("QRTrackerNext." + filename);

            return stream;
        }

        public void ScanSuccess(string text = "扫描成功")
        {
            label.Text = text;
            frame.BackgroundColor = Color.FromHex("#00C853");

            var stream = GetStreamFromFile("ScanSound.mp3");
            var audio = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            audio.Load(stream);
            audio.Play();
        }

        public void ScanFailure(string text = "扫描失败")
        {
            label.Text = text;
            frame.BackgroundColor = Color.FromHex("#FF6659");
            colorButtons.IsVisible = false;
            colorCallback = null;
        }

        public void RequestRateLastScan(Action<string> callback)
        {
            if (defaultColor != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    callback(defaultColor);
                });
            }
            else
            {
                colorButtons.IsVisible = true;
                colorCallback = callback;
            }
        }

        string lastResult = "";

        public CustomScanPage(List<string> colors = null) : base()
        {
            overlay = new Overlay();
            label = new Label() { Text = "请扫描学生二维码" };
            frame = new Frame()
            {
                Content = label,
                BackgroundColor = Color.FromHex("#00C853"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = 20,
                CornerRadius = 3
            };

            colorButtons = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 150),
                Orientation = StackOrientation.Horizontal,
                Spacing = 20,
                IsVisible = false
            };
            if (colors != null && colors.Count > 0)
            {
                foreach (var color in colors)
                {
                    var button = new Button()
                    {
                        BackgroundColor = LabelUtils.NameToAccentXFColor(color),
                        WidthRequest = 50,
                        HeightRequest = 50
                    };
                    button.Clicked += (_, __) =>
                    {
                        if (colorCallback != null)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                colorCallback(color);
                                colorCallback = null;
                            });
                        }
                        else
                        {
                            colorCallback = null;
                        }
                        colorButtons.IsVisible = false;
                    };
                    colorButtons.Children.Add(button);
                }

                ToolbarItems.Add(new ToolbarItem()
                {
                    Text = "设置默认颜色",
                    Command = new Command(async () =>
                    {
                        var color = await UserDialogs.Instance.ActionSheetAsync("选择默认颜色", "不设置默认颜色", null, null,
                            colors.Select(i => LabelUtils.NameToChineseDisplay(i)).ToArray());
                        if (!string.IsNullOrEmpty(color))
                        {
                            if (color != "不设置默认颜色")
                            {
                                defaultColor = LabelUtils.ChineseDisplayToName(color);
                                Title = $"扫一扫 ({color})";
                            }
                            else
                            {
                                defaultColor = null;
                                Title = "扫一扫";
                            }
                        }
                    })
                });
            }

            Title = "扫一扫";

            zxing = new ZXingScannerView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                AutomationId = "zxingScannerView",
                Options = new ZXing.Mobile.MobileBarcodeScanningOptions()
                {
                    PossibleFormats = { ZXing.BarcodeFormat.QR_CODE, ZXing.BarcodeFormat.PDF_417 }
                }
            };

            // 返回结果
            zxing.OnScanResult += (result) =>
            {
                if (result?.Text != lastResult)
                {
                    lastResult = result.Text;
                    Debug.WriteLine(result.Text);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        OnScanResult?.Invoke(result);
                    });
                }
            };

            // 闪光灯
            overlay.Options.FlashTappedAction = () =>
            {
                zxing.IsTorchOn = !zxing.IsTorchOn;
            };

            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    zxing,
                    overlay,
                    frame,
                    colorButtons
                }
            };

            Content = grid;
        }

        // 扫描结果
        public Action<ZXing.Result> OnScanResult { get; set; }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            Shell.SetTabBarIsVisible(this, false);

            zxing.IsScanning = true;

            if (overlay != null && overlay.Options.ShowScanAnimation)
                await overlay.ScanAnimationAsync();
        }

        protected override void OnDisappearing()
        {
            // Seems to be a zxing bug.
            //zxing.IsScanning = false;

            base.OnDisappearing();

            //Shell.SetTabBarIsVisible(this, true);
        }
    }
}