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
    internal class ChartEventArgs
    {
        internal class GraphClickedEventArgs : EventArgs
        {
            internal View View { get; }

            internal float Xpos { get; }

            internal GraphClickedEventArgs(View v, float x)
            {
                View = v;
                Xpos = x;
            }
        }
    }
}