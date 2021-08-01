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
    [QueryProperty(nameof(HomeworkId), "homeworkId")]
    public partial class HomeworkDetailPage : ContentPage
    {
        public string HomeworkId
        {
            set
            {
                BindingContext = viewModel = new HomeworkDetailViewModel(value);
                viewModel.LoadStudentsCommand.Execute(null);
            }
        }

        HomeworkDetailViewModel viewModel;

        public HomeworkDetailPage()
        {
            InitializeComponent();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            viewModel.LoadStudentsCommand.Execute(null);
        }
    }
}