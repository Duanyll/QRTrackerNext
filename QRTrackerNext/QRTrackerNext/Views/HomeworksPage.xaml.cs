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
    public partial class HomeworksPage : ContentPage
    {
        private HomeworksViewModel viewModel;
        public HomeworksPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new HomeworksViewModel();
        }

        private void BrowseHomeworksPage_Appearing(object sender, EventArgs e)
        {
            //viewModel.OnAppearing();
        }

        private void BrowseHomeworksPage_Disappearing(object sender, EventArgs e)
        {
            //viewModel.OnDisappearing();
        }
    }
}