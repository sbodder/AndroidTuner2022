﻿using System;
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
    interface ISerialisableDataPoint
    {

        double XVal { get; set; }
        double YVal { get; set; }

        string PrintHeader(string unit);
        string Print();
        void Recalculate();

    }
}