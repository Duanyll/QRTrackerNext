﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QRTrackerNext.Services
{
    public interface IMediaStore
    {
        void SaveImageFromStream(System.IO.Stream imageStream, string fileName);
        void SaveCSV(string csv, string fileName);
    }
}
