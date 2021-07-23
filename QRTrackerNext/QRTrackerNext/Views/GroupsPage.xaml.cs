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
    public partial class GroupsPage : ContentPage
    {
        private GroupsViewModel viewModel;
        public GroupsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new GroupsViewModel();
        }

        private void BrowseGroupsPage_Appearing(object sender, EventArgs e)
        {
            viewModel.OnAppearing();
        }

        private void BrowseGroupsPage_Disappearing(object sender, EventArgs e)
        {
            viewModel.OnDisappearing();
        }
    }
}