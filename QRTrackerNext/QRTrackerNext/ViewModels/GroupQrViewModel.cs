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

        public new bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                GeneratePicCommand.ChangeCanExecute();
                SavePicCommand.ChangeCanExecute();
            }
        }

        bool usePDF417 = false;
        public bool UsePDF417
        {
            get => usePDF417;
            set
            {
                SetProperty(ref usePDF417, value);
            }
        }

        public ObservableCollection<ImageSource> Images { get; }
        List<SkiaSharp.SKBitmap> bitmaps;

        public Command GeneratePicCommand { get; }
        public Command SavePicCommand { get; }

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

        public GroupQrViewModel(string groupId)
        {
            Images = new ObservableCollection<ImageSource>();
            var realm = RealmManager.OpenDefault();
            var group = realm.Find<Group>(ObjectId.Parse(groupId)).Freeze();

            GeneratePicCommand = new Command(async () =>
            {
                IsBusy = true;
                await Task.Run(() =>
                {
                    bitmaps = UsePDF417 ?
                        QRHelper.GetClassPDF417CodePic(group.Students, WidthCount, HeightCount)
                        : QRHelper.GetClassQrCodePic(group.Students, WidthCount, HeightCount);
                });
                Images.Clear();
                foreach (var i in bitmaps)
                {
                    Images.Add(ImageSource.FromStream(() => i.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream()));
                }
                IsBusy = false;
            }, () => !IsBusy);

            SavePicCommand = new Command(async () =>
            {
                if (bitmaps?.Count > 0 && await CheckPermission())
                {
                    IsBusy = true;
                    
                    await Task.Run(() =>
                    {
                        var store = DependencyService.Get<IMediaStore>();
                        for (int i = 0; i < bitmaps.Count; i++)
                        {
                            store.SaveImageFromStream(bitmaps[i].Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream(), $"{group.Name}-{i + 1}.png");
                        }
                    });
                    UserDialogs.Instance.Toast("已保存到相册 /sdcard/Pictures/QRTracker", new TimeSpan(0, 0, 5));
                    IsBusy = false;
                }
            }, () => !IsBusy && bitmaps?.Count > 0);
        }
    }
}
