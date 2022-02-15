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
    [QueryProperty(nameof(TypeId), "typeId")]

    public partial class HomeworksByTypePage : ContentPage
    {
        public string TypeId
        {
            set
            {
                BindingContext = new HomeworksByTypeViewModel(value);
            }
        }
        public HomeworksByTypePage()
        {
            InitializeComponent();
        }
    }
}