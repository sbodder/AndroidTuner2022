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
    /// <summary>
    /// Standard Datapoint class -  to be re used with different types of data point
    /// </summary>
    [Serializable()]
    public class Serializable_DataPoint_Std : ISerialisableDataPoint
    {
        public Serializable_DataPoint_Std()
        {
            X = 0;
            Y = 0;
        }


        public Serializable_DataPoint_Std(double x, double y)
        {
            X = x;
            Y = y;
        }



        public double X;
        public double Y;

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

        public void Recalculate()
        {

        }
        public string PrintHeader(string unit)
        {
            return string.Format("X (seconds),Y (%s)\n", unit);
        }
        public string Print()
        {
            return string.Format("{0:0.00},{1:0.00}\n", X, Y);
        }
    }
}