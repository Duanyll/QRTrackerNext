﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Xamarin.Forms.LayoutOptions;

namespace QRTrackerNext.Views.ScanningOverlay
{
    class Overlay: Grid
    {
        private BoxView _scanLine;

        // 参数信息
        public Options Options { get; }

        public Overlay(Options options = null)
        {
            Options = options ?? new Options();

            RowSpacing = 0;
            ColumnSpacing = 0;

            VerticalOptions = FillAndExpand;
            HorizontalOptions = FillAndExpand;

            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = Options.ScanHeight });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = Options.ScanWidth });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AddScreenArea();

            AddCenterArea();


            SetRow(Options.TopLabel, 0);
            SetColumnSpan(Options.TopLabel, 3);
            Children.Add(Options.TopLabel);


            SetRow(Options.BottomLabel, 2);
            SetColumnSpan(Options.BottomLabel, 3);
            Children.Add(Options.BottomLabel);



            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (sender, e) =>
            {
                Options.FlashTappedAction?.Invoke();
            };
            Options.FlashLabel.GestureRecognizers.Add(tapGestureRecognizer);

            if (Options.ShowFlash)
                Children.Add(Options.FlashLabel, 2, 2);
        }

        private void AddScreenArea()
        {
            var box1 = new BoxView
            {
                VerticalOptions = Fill,
                HorizontalOptions = FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            };
            SetRow(box1, 0);
            SetColumnSpan(box1, 3);
            Children.Add(box1);

            var box2 = new BoxView
            {
                VerticalOptions = Fill,
                HorizontalOptions = FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            };
            SetRow(box2, 2);
            SetColumnSpan(box2, 3);
            Children.Add(box2);

            var box3 = new BoxView
            {
                VerticalOptions = Fill,
                HorizontalOptions = FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            };
            SetRow(box3, 1);
            SetColumn(box3, 0);
            Children.Add(box3);

            var box4 = new BoxView
            {
                VerticalOptions = Fill,
                HorizontalOptions = FillAndExpand,
                BackgroundColor = Color.Black,
                Opacity = 0.7,
            };
            SetRow(box4, 1);
            SetColumn(box4, 2);
            Children.Add(box4);
        }

        // 添加中心区域
        private void AddCenterArea()
        {
            #region 扫描动画

            _scanLine = GetLine(Start, Center, (box) =>
            {
                box.HeightRequest = 2;
                box.WidthRequest = Options.ScanWidth - 6;
                box.BackgroundColor = Options.ScanColor;
                box.Opacity = 0.6;
            });

            #endregion

            #region 边框

            var topLine = GetLine(Start, FillAndExpand, (box) =>
            {
                box.HeightRequest = 1;
            });

            var leftLine = GetLine(FillAndExpand, Start, (box) =>
            {
                box.WidthRequest = 1;
            });

            var rightLine = GetLine(FillAndExpand, End, (box) =>
            {
                box.WidthRequest = 1;
            });

            var bottomLine = GetLine(End, FillAndExpand, (box) =>
            {
                box.HeightRequest = 1;
            });

            #endregion

            #region 四角边

            double frThick = 3; // 厚度
            double frLength = 20; // 长度

            var tllLine = GetLine(Start, Start, (box) =>
            {
                box.WidthRequest = frThick;
                box.HeightRequest = frLength;
            });

            var tltLine = GetLine(Start, Start, (box) =>
            {
                box.HeightRequest = frThick;
                box.WidthRequest = frLength;
            });

            var trtLine = GetLine(Start, End, (box) =>
            {
                box.HeightRequest = frThick;
                box.WidthRequest = frLength;
            });

            var trrLine = GetLine(Start, End, (box) =>
            {
                box.WidthRequest = frThick;
                box.HeightRequest = frLength;
            });

            var bllLine = GetLine(End, Start, (box) =>
            {
                box.WidthRequest = frThick;
                box.HeightRequest = frLength;
            });

            var blbLine = GetLine(End, Start, (box) =>
            {
                box.HeightRequest = frThick;
                box.WidthRequest = frLength;
            });

            var brbLine = GetLine(End, End, (box) =>
            {
                box.HeightRequest = frThick;
                box.WidthRequest = frLength;
            });

            var brrLine = GetLine(End, End, (box) =>
            {
                box.WidthRequest = frThick;
                box.HeightRequest = frLength;
            });

            #endregion

            BoxView GetLine(LayoutOptions ver, LayoutOptions hor,
                Action<View> action = null)
            {
                var boxView = new BoxView
                {
                    VerticalOptions = ver,
                    HorizontalOptions = hor,
                    BackgroundColor = Options.ScanColor,
                };

                SetPlace(boxView);

                action?.Invoke(boxView);

                return boxView;
            }

            void SetPlace(View view)
            {
                SetRow(view, 1);
                SetColumn(view, 1);
                Children.Add(view);
            }
        }

        // 扫描动画
        public async Task ScanAnimationAsync()
        {
            while (_scanLine != null)
            {
                await _scanLine.TranslateTo(0, Options.ScanHeight - 3, 3000, Easing.CubicInOut);
                await _scanLine.TranslateTo(0, 1, 3000, Easing.CubicInOut);
            }
        }
    }
}
