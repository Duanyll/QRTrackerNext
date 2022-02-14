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

    public partial class EditHomeworkTypePage : ContentPage
    {
        public string TypeId
        {
            set
            {
                BindingContext = new EditHomeworkTypeViewModel(value);
            }
        }
        public EditHomeworkTypePage()
        {
            InitializeComponent();
        }
    }
}