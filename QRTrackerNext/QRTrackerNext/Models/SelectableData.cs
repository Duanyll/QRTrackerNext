using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QRTrackerNext.Models
{
    public class SelectableData<T> : ViewModels.NotifyPropertyChanged
    {
        public T Data { get; set; }
        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set => SetProperty(ref selected, value);
        }

        public SelectableData(T data) { Data = data; }
    }
}
