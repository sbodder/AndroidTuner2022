using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;

namespace TTtuner_2022_2.Plot
{
    /// <summary>
    /// Datapoint class - this is specific to the frequency datapoint
    /// </summary>
    [Serializable()]
    public class Serializable_DataPoint : ISerialisableDataPoint, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Serializable_DataPoint()
        {
            XVal = 0;
            YVal = 0;
        }

        public Serializable_DataPoint(double x, double y)
        {
            string strNote;
            double dblCentsCloseness = 0;
            strNote = Music.NotePitchMap.GetNoteFromPitch(y, ref dblCentsCloseness);
            XVal = x;
            YVal = y;
            PctCloseness = dblCentsCloseness;
            Note = strNote;
        }

        public Serializable_DataPoint(double x, double y, double pctCloseness, string strNote)
        {
            XVal = x;
            YVal = y;
            PctCloseness = pctCloseness;
            Note = strNote;
        }

        public void Recalculate()
        {
            string strNote;
            double dblCentsCloseness = 0;
            strNote = Music.NotePitchMap.GetNoteFromPitch(Y, ref dblCentsCloseness);
            PctCloseness = dblCentsCloseness;
            Note = strNote;
        }

        public double X;
        public double Y;
        public double PctCloseness;
        public string Note;

        public double CentsDeviation
        {
            get
            {
                return PctCloseness;
            }
            set
            {
                PctCloseness = value;
            }
        }

        public double XVal
        {
            get
            {
                return X;
            }
            set
            {
                X = value;
            }
        }

        public double YVal
        {
            get
            {
                return Y;
            }
            set
            {
                Y = value;
            }
        }

        public string Print()
        {
            return string.Format("{0:0.00} , {1:0.00}, {2}, {3:0.00}\n", XVal, YVal, Note, PctCloseness);

        }
        public string PrintHeader(string unit)
        {
            return "X (seconds),Y (Hertz),Note,Cents Error (Cents)\n";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}