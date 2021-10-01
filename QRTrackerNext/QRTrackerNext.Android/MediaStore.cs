using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using QRTrackerNext.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(QRTrackerNext.Droid.MediaStore))]
namespace QRTrackerNext.Droid
{
    class MediaStore : IMediaStore
    {
        public void SaveImageFromStream(Stream imageStream, string fileName)
        {
            var dir = Path.Combine("/storage/emulated/0/Pictures/QRTracker");
            var path = Path.Combine(dir, fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var fileStream = File.OpenWrite(path))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                imageStream.CopyTo(fileStream);
            }
        }

        public void SaveCSV(string csv, string fileName)
        {
            var dir = Path.Combine("/storage/emulated/0/Document/QRTracker");
            var path = Path.Combine(dir, fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, csv);
        }
    }
}