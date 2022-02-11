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
using Xamarin.Essentials;

using QRTrackerNext.Models;
using QRTrackerNext.Views;

namespace QRTrackerNext.ViewModels
{
    class StudentDetailViewModel : BaseViewModel
    {
        Student student;
        string name;
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        string uri;
        public string Uri
        {
            get => uri;
            set
            {
                SetProperty(ref uri, value);
            }
        }

        string uriShort;
        public string UriShort
        {
            get => uriShort;
            set
            {
                SetProperty(ref uriShort, value);
            }
        }

        public StudentDetailViewModel(string studentId)
        {
            var realm = Services.RealmManager.OpenDefault();
            student = realm.Find<Student>(ObjectId.Parse(studentId));
            Name = Title = student.Name;
            Uri = QRHelper.GetStudentUri(student);
            UriShort = QRHelper.GetStudentUriShort(student);
        }
    }
}
