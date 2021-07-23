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
    public partial class StudentsPage : ContentPage
    {

        public string GroupId
        {
            set
            {
                BindingContext = viewModel = new StudentsViewModel(value);
                viewModel.OnAppearing();
            }
        }

        private StudentsViewModel viewModel;
        public StudentsPage()
        {
            InitializeComponent();
        }

        private void BrowseStudentsPage_Appearing(object sender, EventArgs e)
        {
            viewModel?.OnAppearing();
        }

        private void BrowseStudentsPage_Disappearing(object sender, EventArgs e)
        {
            viewModel?.OnDisappearing();
        }
    }
}