using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using QRTrackerNext.ViewModels;

namespace QRTrackerNext.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(StudentId), "studentId")]
    public partial class StudentDetailPage : ContentPage
    {
        public string StudentId
        {
            set
            {
                BindingContext = viewModel = new StudentDetailViewModel(value);
            }
        }

        private StudentDetailViewModel viewModel;
        public StudentDetailPage()
        {
            InitializeComponent();
        }
    }
}