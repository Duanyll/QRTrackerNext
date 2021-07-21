using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class CustomScanPage : ContentPage
    {
        private ZXingScannerView zxing;
        private Overlay overlay;

        public CustomScanPage(Overlay overlay = null) : base()
        {
            this.overlay = overlay ?? new Overlay();

            Title = "扫一扫";

            zxing = new ZXingScannerView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                AutomationId = "zxingScannerView",
            };

            // 返回结果
            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                {
                    zxing.IsAnalyzing = false;

                    await Shell.Current.GoToAsync("..");

                    OnScanResult?.Invoke(result);
                });

            // 闪光灯
            this.overlay.Options.FlashTappedAction = () =>
            {
                zxing.IsTorchOn = !zxing.IsTorchOn;
            };

            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            grid.Children.Add(zxing);
            grid.Children.Add(this.overlay);

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
            zxing.IsScanning = false;

            base.OnDisappearing();

            //Shell.SetTabBarIsVisible(this, true);
        }
    }
}