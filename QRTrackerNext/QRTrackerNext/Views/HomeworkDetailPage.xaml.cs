using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microcharts;

using QRTrackerNext.Models;
using QRTrackerNext.ViewModels;

namespace QRTrackerNext.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(HomeworkId), "homeworkId")]
    public partial class HomeworkDetailPage : TabbedPage
    {
        HomeworkDetailViewModel viewModel;
        public string HomeworkId
        {
            set
            {
                BindingContext = viewModel = new HomeworkDetailViewModel(value);
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

        private void StatsPage_Appearing(object sender, EventArgs e)
        {
            var entries = viewModel.GetStatsChartEntry();
            statsChartView.Chart = new PieChart()
            {
                IsAnimated = true,
                LabelMode = LabelMode.None,
                Entries = entries
            };
            statsLabelCollectionView.ItemsSource = entries;
        }
    }
}