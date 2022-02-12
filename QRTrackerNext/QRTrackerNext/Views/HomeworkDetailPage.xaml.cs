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
    public partial class HomeworkDetailPage : TabbedPage
    {
        public string HomeworkId
        {
            set
            {
                BindingContext = new HomeworkDetailViewModel(value);
            }
        }

        public HomeworkDetailPage()
        {
            InitializeComponent();
        }

        private void TabbedPage_Appearing(object sender, EventArgs e)
        {
        }

        private void TextCellSubmitted_Tapped(object sender, EventArgs e)
        {
            CurrentPage = Children[1];
        }

        private void TextCellNotSubmitted_Tapped(object sender, EventArgs e)
        {
            CurrentPage = Children[2];
        }
    }
}