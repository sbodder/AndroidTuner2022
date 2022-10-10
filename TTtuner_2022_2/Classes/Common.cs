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

namespace TTtuner_2022_2.Android.Common
{
    class Common
    {
    }

    public class NoteEventArgs : EventArgs
    {
        private string strNote;
        private double dblPrctCloseness;
        public NoteEventArgs(string strNoteValue, double dblPercentClosness)
        {
            strNote = strNoteValue;
            dblPrctCloseness = dblPercentClosness;
        }

        public string Note { get { return strNote; } }
        public double PercentCloseness { get { return dblPrctCloseness; } }
    } 
}