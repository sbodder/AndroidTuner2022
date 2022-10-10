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
using TTtuner_2022_2.Music;

namespace TTtuner_2022_2.Plot
{
    internal class GraphHelper_Freq
    {
        const int LOG_MULT = 100;
        int m_intOffset = (int)(5 * (LOG_MULT * Math.Log10(NotePitchMap.oct3[4]) - LOG_MULT * Math.Log10(NotePitchMap.oct3[3])));


        internal double LogValueForGraph_Get(double freq)
        {
            return LOG_MULT * Math.Log10(freq);
            //return freq;
        }

        internal double InverseLogValueForGraph_Get(double yVal)
        {
            return Math.Pow(10, yVal / (double) LOG_MULT);
            // return yVal;
        }

        internal int OffsetForGraph_Get()
        {
            return m_intOffset;
            //return 60;
        }
    }
}