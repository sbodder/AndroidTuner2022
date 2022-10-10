using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;

namespace TTtuner_2022_2.Plot
{
    internal interface IDataPointHelper
    {
        void AddDataPointToCollection(ISerialisableDataPoint dp);
        void AddDataPointToCollection(double x, double y);
        bool SaveDataPointsToFile(string strFileName);
        int NumberOfDataPoints { get; }
    }
}