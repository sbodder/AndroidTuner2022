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
using Com.Syncfusion.Charts;

namespace TTtuner_2022_2.Fragments
{
    class TimeAxisForScatterFrag : NumericalAxis
    {
        protected override void GenerateVisibleLabels()
        {
            base.GenerateVisibleLabels();
            VisibleLabels.Clear();
        }
    }

    class CentsAxisForScatterFrag : NumericalAxis
    {
        protected override void GenerateVisibleLabels()
        {
            base.GenerateVisibleLabels();
            VisibleLabels.Clear();

            VisibleLabels.Add(new ChartAxisLabel(-25, "-25"));
            VisibleLabels.Add(new ChartAxisLabel(0, "0"));
            VisibleLabels.Add(new ChartAxisLabel(25, "25"));
        }
    }
}