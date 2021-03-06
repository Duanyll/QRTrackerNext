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
    [QueryProperty(nameof(TypeId), "typeId")]
    public partial class NewHomeworkPage : ContentPage
    {
        NewHomeworkViewModel viewModel;
        public string TypeId
        {
            set
            {
                viewModel.SetHomeworkTypeById(value);
            }
        }
        public NewHomeworkPage()
        {
            BindingContext = viewModel = new NewHomeworkViewModel();
            InitializeComponent();
        }

        private void SwitchCell_OnChanged(object sender, ToggledEventArgs e)
        {
            viewModel.CreateNewHomeworkCommand.ChangeCanExecute();
        }
    }
}