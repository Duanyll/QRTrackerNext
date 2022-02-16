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
    [QueryProperty(nameof(GroupId), "groupId")]
    public partial class GroupStatsPage : TabbedPage
    {
        public string GroupId
        {
            set
            {
                BindingContext = viewModel = new GroupStatsViewModel(value);
            }
        }

        GroupStatsViewModel viewModel;
        public GroupStatsPage()
        {
            InitializeComponent();
        }

        private void SwitchCell_OnChanged(object sender, ToggledEventArgs e)
        {
        }

        private void homeworkListPage_Appearing(object sender, EventArgs e)
        {
            viewModel.ResetFilterOptions();
        }
    }
}