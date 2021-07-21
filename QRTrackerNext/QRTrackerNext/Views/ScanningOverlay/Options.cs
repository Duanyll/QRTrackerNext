using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class Options
    {
        public Options()
        {
            TopLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
                AutomationId = "zxingDefaultOverlay_TopTextLabel",
            };

            BottomLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.White,
                AutomationId = "zxingDefaultOverlay_BottomTextLabel",
            };

            FlashLabel = new Label
            {
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Text = "闪光灯",
                FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)),
                TextColor = Color.White,
                AutomationId = "zxingDefaultOverlay_FlashButton",
                Margin = new Thickness(8)
            };
        }

        // 顶部标签
        public Label TopLabel { get; private set; }
        // 底部标签
        public Label BottomLabel { get; private set; }
        // 闪光灯
        public Label FlashLabel { get; private set; }
        // 闪光灯点击操作
        public Action FlashTappedAction { get; set; }

        // 扫描框大小
        public double ScanWidth { get; set; } = 230;
        public double ScanHeight { get; set; } = 230;

        // 扫描光及扫描线的颜色
        public Color ScanColor { get; set; } = Color.White;
        // 是否显示闪光灯
        public bool ShowFlash { get; set; }
        // 是否显示扫描动画
        public bool ShowScanAnimation { get; set; } = true;
    }
}
