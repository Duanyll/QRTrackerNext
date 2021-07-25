using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using QRTrackerNext.Models;
using QRTrackerNext.ViewModels;

namespace QRTrackerNext.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class GroupQrPage : ContentPage
    {
        public string GroupId
        {
            set
            {
                viewModel.groupId = value;
            }
        }

        GroupQrViewModel viewModel;
        public GroupQrPage()
        {
            BindingContext = viewModel = new GroupQrViewModel();
            InitializeComponent();
        }
    }
}