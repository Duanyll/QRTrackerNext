using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using System.IO;
using System.Reflection;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class CustomScanPage : ContentPage
    {
        private ZXingScannerView zxing;
        private Overlay overlay;

        private Label label;
        private Frame frame;

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
        }

        string lastResult = "";

        public CustomScanPage() : base()
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
                    Device.BeginInvokeOnMainThread(async () =>
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
                    frame
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