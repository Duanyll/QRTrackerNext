using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class CustomScanPage : ContentPage
    {
        private ZXingScannerView zxing;
        private Overlay overlay;

        private Label label;
        private Frame frame;

        public string LabelText {
            get => label.Text;
            set => label.Text = value;
        }

        string lastResult = "";

        public CustomScanPage() : base()
        {
            overlay = new Overlay();
            label = new Label() { Text = "请扫描学生二维码" };
            frame = new Frame()
            {
                Content = label,
                BackgroundColor = Color.Accent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = 10,
                CornerRadius = 10
            };

            Title = "扫一扫";

            zxing = new ZXingScannerView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                AutomationId = "zxingScannerView",
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