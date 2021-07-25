using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using MongoDB.Bson;
using Acr.UserDialogs;

using Xamarin.Forms;
//using Xamarin.Essentials;

using QRTrackerNext.Models;
using QRTrackerNext.Views;
using QRTrackerNext.Services;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace QRTrackerNext.ViewModels
{
    class GroupQrViewModel : BaseViewModel
    {
        int widthCount = 4;
        public int WidthCount
        {
            get => widthCount;
            set
            {
                SetProperty(ref widthCount, value);
            }
        }

        int heightCount = 5;
        public int HeightCount
        {
            get => heightCount;
            set
            {
                SetProperty(ref heightCount, value);
            }
        }

        bool canSavePic = false;
        public bool CanSavePic
        {
            get => canSavePic;
            set
            {
                SetProperty(ref canSavePic, value);
            }
        }

        public ObservableCollection<ImageSource> Images { get; }
        List<SkiaSharp.SKBitmap> bitmaps;

        public Command GeneratePicCommand { get; }
        public Command SavePicCommand { get; }

        public string groupId { get; set; }

        private async Task<bool> CheckPermission()
        {
            var current = CrossPermissions.Current;
            var status = await current.CheckPermissionStatusAsync<StoragePermission>();
            if (status != PermissionStatus.Granted)
            {
                status = await current.RequestPermissionAsync<StoragePermission>();
            }
            return status == PermissionStatus.Granted;
        }

        public GroupQrViewModel()
        {
            Images = new ObservableCollection<ImageSource>();

            GeneratePicCommand = new Command(() =>
            {
                var realm = Realm.GetInstance();
                var group = realm.Find<Group>(ObjectId.Parse(groupId));
                List<(string Id, string Name)> stuList = new List<(string Id, string Name)>();
                foreach (var i in group.Students)
                {
                    stuList.Add((i.Id.ToString(), i.Name));
                }
                Task.Run(() =>
                {
                    IsBusy = true;
                    canSavePic = false;
                    var skbitmaps = QRHelper.GetClassQrCodePic(stuList, WidthCount, HeightCount);
                    bitmaps = skbitmaps;
                    Images.Clear();
                    foreach (var i in skbitmaps)
                    {
                        Images.Add(ImageSource.FromStream(() => i.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream()));
                    }
                    IsBusy = false;
                    canSavePic = true;
                });
            });

            SavePicCommand = new Command(async () =>
            {
                if (bitmaps?.Count > 0 && await CheckPermission())
                {
                    await Task.Run(() =>
                    {
                        canSavePic = false;

                        var realm = Realm.GetInstance();
                        var group = realm.Find<Group>(ObjectId.Parse(groupId));
                        var store = DependencyService.Get<IMediaStore>();
                        for (int i = 0; i < bitmaps.Count; i++)
                        {
                            store.SaveImageFromStream(bitmaps[i].Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream(), $"{group.Name}-{i + 1}.png");
                        }

                        canSavePic = true;
                    });
                }
            });
        }
    }
}
