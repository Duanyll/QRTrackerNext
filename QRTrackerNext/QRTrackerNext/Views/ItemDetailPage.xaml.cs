using QRTrackerNext.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace QRTrackerNext.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}