﻿using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;

namespace QRTrackerNext.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRPreviewPage : ContentPage
    {
        public QRPreviewPage()
        {
            InitializeComponent();
        }

        private async Task<bool> CheckPermission()
        {
            var current = CrossPermissions.Current;
            var status = await current.CheckPermissionStatusAsync<CameraPermission>();
            if (status != PermissionStatus.Granted)
            {
                status = await current.RequestPermissionAsync<CameraPermission>();
            }
            return status == PermissionStatus.Granted;
        }

        private async void btnScanQR_Clicked(object sender, EventArgs e)
        {
            if (await CheckPermission())
            {
                var scanner = new MobileBarcodeScanner();
                var result = await scanner.Scan();
                if (null != result)
                {
                    await DisplayAlert("扫描结果", result.Text, "确定");
                }
            }
        }
    }
}