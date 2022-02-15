﻿using QRTrackerNext.ViewModels;
using QRTrackerNext.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace QRTrackerNext
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(StudentsPage), typeof(StudentsPage));
            Routing.RegisterRoute(nameof(StudentDetailPage), typeof(StudentDetailPage));
            Routing.RegisterRoute(nameof(GroupQrPage), typeof(GroupQrPage));
            Routing.RegisterRoute(nameof(NewHomeworkPage), typeof(NewHomeworkPage));
            Routing.RegisterRoute(nameof(HomeworkDetailPage), typeof(HomeworkDetailPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(GroupStatsPage), typeof(GroupStatsPage));
            Routing.RegisterRoute(nameof(EditHomeworkTypePage), typeof(EditHomeworkTypePage));
            Routing.RegisterRoute(nameof(HomeworksPage), typeof(HomeworksPage));
            Routing.RegisterRoute(nameof(HomeworkTypesPage), typeof(HomeworkTypesPage));
            Routing.RegisterRoute(nameof(HomeworksByTypePage), typeof(HomeworksByTypePage));
        }

    }
}
