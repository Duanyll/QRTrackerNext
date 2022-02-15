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
    public partial class HomeworkTypesPage : ContentPage
    {
        public HomeworkTypesPage()
        {
            BindingContext = new HomeworkTypesViewModel();
            InitializeComponent();
        }
    }
}