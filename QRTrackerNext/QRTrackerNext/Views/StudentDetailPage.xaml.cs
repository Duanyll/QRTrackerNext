using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microcharts;

using QRTrackerNext.ViewModels;

namespace QRTrackerNext.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(StudentId), "studentId")]
    public partial class StudentDetailPage : TabbedPage
    {
        public string StudentId
        {
            set
            {
                BindingContext = viewModel = new StudentDetailViewModel(value);
                viewModel.UpdateChart = () =>
                {
                    statsChart.Chart = new PieChart
                    {
                        LabelMode = LabelMode.None,
                        IsAnimated = true,
                        Entries = viewModel.ChartEntries
                    };
                };
            }
        }

        private StudentDetailViewModel viewModel;
        public StudentDetailPage()
        {
            InitializeComponent();
        }
    }
}