using System;
using System.Collections.Generic;
using System.Text;

namespace QRTrackerNext.Models
{
    public class SelectableData<T>
    {
        public T Data { get; set; }
        public bool Selected { get; set; } = false;

        public SelectableData(T data) { Data = data; }
    }
}
