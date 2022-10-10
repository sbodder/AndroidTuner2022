using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using TTtuner_2022_2.Plot;

namespace TTtuner_2022_2.Music
{
    public class DbStatsGenerator
    {
        //ObservableCollection<DbStats> Stats { get; set; }
        private int MIN_NUMBER_OF_SAMPLES = Common.Settings.MinNumberOfSamplesForNote;

        public class DbStat2
        {
            private double _dbMean = 44;
            public string Mean
            {
                get
                {
                    return ((int)_dbMean).ToString();
                }
            }
        }
        public class DbStat
        {
            public int Mean
            {
                get
                {
                    return (int)_dbMean;
                }
            }
            public int Samples { get; set; }
            public int Max { get; set; }

            private double _dbMean;

            private List<double> m_lstDbValues = new List<double>();
            public DbStat()
            {
                Samples = 0;
                _dbMean = 0;
                Max = int.MinValue;
            }

            public int MinValue()
            {

                // want to scale to be between 0 (max) and -150 (min)
                // for all db graphs
                return (int)-150;
            }


            public int RangeOfvalues()
            {
                // the max value on a dbFS scale is always 0 
                // so we can just return the abs of teh min value
                return Math.Abs(MinValue());
            }


            // averagenew= averageold + (valuenew − averageold) / sizenew
            public void DbValue_Add(double increase)
            {
                Samples++;
                m_lstDbValues.Add(increase);
                if (increase > Max)
                {
                    Max = (int)increase;
                }
                _dbMean = _dbMean + ((increase - _dbMean) / Samples);
            }

            internal double LowerInterQuartileRange
            {
                get
                {
                    double val = FindInterQuartileValue(0.25f);
                    // get back actual interq value
                    val = -RangeOfvalues() + val;
                    return val;
                }
            }

            internal double UpperInterQuartileRange
            {
                get
                {
                    double val = FindInterQuartileValue(0.75f);
                    // get back actual interq value
                    val = -RangeOfvalues() + val;
                    return val;
                }
            }

            public int Percentile95
            {
                get
                {
                    double val = FindInterQuartileValue(0.95f);
                    // get back actual interq value
                    val = -RangeOfvalues() + val;
                    return (int)val;
                }
            }





            public Bitmap Image
            {
                get
                {
                    float x1, y1, x2, y2;
                    const int PADDING = 5;

                    const int BITMAP_WIDTH = 500;
                    float MULT_FACTOR;
                    const int BITMAP_HEIGHT = 200;
                    const int Y_PADDING = 30;
                    const float CIRCLE_RADIUS = 20f;
                    Bitmap bitmap = Bitmap.CreateBitmap(BITMAP_WIDTH, BITMAP_HEIGHT, Bitmap.Config.Argb8888);
                    Canvas canvas = new Canvas(bitmap);
                    Paint paint = new Paint();

                    // the max value for db FS will always be 0
                    // but wish to calculate range over max - min
                    //  int rangeOfvalues = Math.Abs( Max-MinValue());

                    MULT_FACTOR = BITMAP_WIDTH / RangeOfvalues();

                    canvas.DrawARGB(0, 0, 0, 0);
                    paint.AntiAlias = true;
                    paint.SetStyle(Paint.Style.Fill);


                    // draw rectangle
                    paint.Color = Color.LightGray;

                    x1 = (float)(FindInterQuartileValue(0.25f) * MULT_FACTOR);
                    if (x1 < 0)
                    {
                        x1 = 0;
                    }
                    y1 = Y_PADDING;

                    x2 = (float)(FindInterQuartileValue(0.75f) * MULT_FACTOR);

                    if (x2 > BITMAP_WIDTH)
                    {
                        x2 = BITMAP_WIDTH;
                    }
                    y2 = BITMAP_HEIGHT - Y_PADDING;

                    canvas.DrawRect(x1, y1, x2, y2, paint);

                    // draw  circle
                    paint.Color = Color.Black;
                    x1 = (float)(Math.Abs(-RangeOfvalues() - Mean) * MULT_FACTOR);
                    y1 = BITMAP_HEIGHT / 2;
                    canvas.DrawCircle(x1, y1, CIRCLE_RADIUS, paint);


                    // draw center line
                    paint.Color = Color.Gray;
                    paint.SetStyle(Paint.Style.Stroke);
                    paint.StrokeWidth = 10f;

                    canvas.DrawLine(BITMAP_WIDTH / 2, 0, BITMAP_WIDTH / 2, BITMAP_HEIGHT, paint);

                    // draw circumference lines

                    canvas.DrawLine(PADDING, 0, PADDING, BITMAP_HEIGHT, paint);
                    canvas.DrawLine(BITMAP_WIDTH - PADDING, 0, BITMAP_WIDTH - PADDING, BITMAP_HEIGHT, paint);

                    canvas.DrawLine(0, 0, BITMAP_WIDTH, 0, paint);
                    canvas.DrawLine(0, BITMAP_HEIGHT, BITMAP_WIDTH, BITMAP_HEIGHT, paint);

                    canvas.Dispose();
                    canvas = null;
                    paint.Dispose();
                    paint = null;

                    return bitmap;
                }

                set
                {

                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="percent"> Should be between 0 and 1</param>
            /// <returns></returns>
            private double FindInterQuartileValue(float percent)
            {
                int intValNumber;

                if ((percent < 0) || (percent > 1))
                {
                    throw new Exception("FindQuartileValue function - value passed is not between 0 and 1");
                }

                intValNumber = (int)(m_lstDbValues.Count * percent);
                m_lstDbValues = m_lstDbValues.OrderBy(dbl => dbl).ToList();

                // all values in the list are neagative
                // for the purpose of drawing the rectangle make get the difference 
                // between this and the full range of values and return a postive value
                return Math.Abs(-RangeOfvalues() - m_lstDbValues[intValNumber]);
            }


        }

        internal ObservableCollection<DbStat> GenerateStats_Db(ObservableCollection<ISerialisableDataPoint> lstInput)
        {
            DbStat statsRet = new DbStat();
            ObservableCollection<DbStat> lstDbVal = new ObservableCollection<DbStat>();
            List<Serializable_DataPoint_Std> lstInput_dcb = lstInput.ToList().ConvertAll(o => (Serializable_DataPoint_Std)o);

            foreach (Serializable_DataPoint_Std dp in lstInput_dcb)
            {
                statsRet.DbValue_Add(dp.YVal);
            }
            lstDbVal.Add(statsRet);
            return lstDbVal;
        }
    }
}