using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using Com.Syncfusion.Gauges.SfCircularGauge;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Fragments
{
    class GaugeFragment : AndroidX.Fragment.App.Fragment
    {
        const int m_BUFFER_ARRAY_SIZE = 10;
        private int intCurrentArrayIndex = 0;
        double[] m_dlArray = new double[m_BUFFER_ARRAY_SIZE];
        private View m_view;
        private SfCircularGauge _gauge;
        private Activity m_act;

        public bool SetupComplete { get; set; } = false;

        public GaugeFragment()
        {

        }
        public static GaugeFragment NewInstance(Activity act)
        {
            var gaugeFrag = new GaugeFragment
            {
                Arguments = new Bundle(),
                m_act = act
            };

            return gaugeFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            m_view = inflater.Inflate(Resource.Layout.GaugeFragment, container, false);
            SetupGauge();
            SetupComplete = true;
            return m_view;
        }

        private void SetupGauge()
        {
            try
            {
                LinearLayout llay = (LinearLayout)m_view.FindViewById(Resource.Id.LinearLayoutForGauge);
                _gauge = new SfCircularGauge(m_act);

                _gauge.SetBackgroundColor(Color.Black);
                _gauge.SetPadding(0, 0, 0, 0);


                //Initializing scales for circular gauge
                // controls the text, text color and rim color 
                // etc of the rim around the gauge
                ObservableCollection<CircularScale> scales = new ObservableCollection<CircularScale>();
                CircularScale scale = new CircularScale();
                scale.StartValue = -80;
                scale.EndValue = 0;
                scale.StartAngle = 180;
                scale.SweepAngle = 180;
                scale.RimColor = Color.Rgb(0x99, 0xcc, 0);
                scale.RimWidth = 10;
                scale.LabelTextSize = 10;
                scale.LabelColor = Color.White;
                scale.CustomLabels = new double[] { -80, -60, -20, 0 };
                scale.LabelOffset = 0.95;
                TickSetting majorTicks = new TickSetting();
                majorTicks.StartOffset = 0.83;
                majorTicks.EndOffset = 0.90;
                scale.MajorTickSettings = majorTicks;
                TickSetting minorTicks = new TickSetting();
                minorTicks.StartOffset = 0.83;
                minorTicks.EndOffset = 0.88;
                scale.MinorTickSettings = minorTicks;
                scale.ScaleStartOffset = 0.83;
                scale.ScaleEndOffset = 0.73;

                scales.Add(scale);


                //Adding header 
                Header header = new Header();
                header.Text = "dB";
                header.TextSize = 10;
                header.TextColor = Color.White;
                _gauge.Headers.Add(header);

                //Adding range - adds a range marker inside the rim of the scale
                CircularRange range = new CircularRange();
                range.StartValue = -20;
                range.EndValue = 0;
                range.Color = Color.Red;
                range.Width = 50;
                range.Offset = 0.83;
                range.Width = 6;
                //range.InnerEndOffset = 
                //range.Offset = 10;
                scale.CircularRanges.Add(range);

                //Adding needle pointer - this is the inside needle
                NeedlePointer needlePointer = new NeedlePointer();
                needlePointer.Value = -20;
                needlePointer.Color = Color.White;
                scale.CircularPointers.Add(needlePointer);

                _gauge.CircularScales = scales;

                llay.AddView(_gauge);
            }
            catch (Exception e)
            {
                Common.CommonFunctions comFunc = new Common.CommonFunctions();
                //FireBaseEventLogger fb = new FireBaseEventLogger(m_act);
                //fb.SendEvent(fb.events.GAUGE_FRAG_CREATE_EXCEP, comFunc.TruncateStringRight(e.Message, 99));
                throw e;
            }

        }



        public void SetGaugePointerValue(double val)
        {

            UpdateDbLevelArray(val);
            _gauge.CircularScales[0].CircularPointers[0].Value = GetAvgValueOfBuffer();
        }

        internal double GetAvgValueOfBuffer()
        {
            double dbTotal = 0;
            for (int i = 0; i < m_BUFFER_ARRAY_SIZE; i++)
            {
                dbTotal += m_dlArray[i];
            }

            return dbTotal / m_BUFFER_ARRAY_SIZE;
        }

        private void UpdateDbLevelArray(double dbLevel)
        {
            m_dlArray[intCurrentArrayIndex] = dbLevel;

            intCurrentArrayIndex++;

            if (intCurrentArrayIndex > m_BUFFER_ARRAY_SIZE - 1)
            {
                intCurrentArrayIndex = 0;
            }
        }
    }
}