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
using Android.Util;
using global::Android.Views;
using global::Android.Widget;
using Com.Syncfusion.Charts;
using TTtuner_2022_2.Common;
using TTtuner_2022_2.Music;
using TTtuner_2022_2.PopUpDialogFragments;

namespace TTtuner_2022_2.Plot
{
    internal class SfChartMainExt_Freq : SfChart, View.IOnTouchListener, IChart
    {
        const int INDEX_OF_DS_ON_CHART = 1;
        const int INDEX_OF_VL_ON_CHART = 0;
        const int INDEX_OF_FIRST_NM_ON_CHART = 2;
        const int INDEX_OF_VERTICAL_LINE_MAIN_CHART = 0;
        const double ZOOM_OFFSET_X = 0.75d;
        const double ZOOM_OFFSET_Y = 0.12d;
        double _dblLastYpositionOnGraph = double.MaxValue;
        const double Y_DIFFERENCE_GRAPH_THRESHOLD = 0.5d;
        private const int NUM_NOTES_PER_OCTAVE = 12;
        internal const double  MAX_Y_VAL = NotePitchMap.PITCH_HIGH_LIMIT;
        internal const double MIN_Y_VAL = NotePitchMap.PITCH_LOW_LIMIT;
        bool m_needToValidate = true;
        double _maxFreqVal;
        double _minFreqValue = 0;
        ObservableCollection<FastLineSeries> m_lstFastLineSeriesNoteMarkers = new ObservableCollection<FastLineSeries>();
        private int m_intStartIndex;
        ObservableCollection<ISerialisableDataPoint> m_lstPitchPoints = new ObservableCollection<ISerialisableDataPoint>();

        private Activity m_parentActivity;

        public ObservableCollection<ISerialisableDataPoint> DataPoints
        {
            get
            {
                 return m_lstPitchPoints;
            }
        }

        public event EventHandler<ChartEventArgs.GraphClickedEventArgs> GraphClick;
        public event EventHandler<View.TouchEventArgs> Touch;
        public event EventHandler<ZoomStartEventArgs> ZoomStart;
        public event EventHandler<ZoomEndEventArgsCustom> ZoomEnd;

        public float TimeWindowStart { get; }
        const float GRAPH_TAP_ALLOWABLE_PERCENT_ERROR = 0.02f;
        float m_x1;
        float m_y1;
        private int m_intEndIndex;
        private float m_dbMaxXValOnChart;

        ChartZoomPanBehavior chartZoomPanBehavior;

        public View View
        {
            get
            {
                return this;
            }
        }

        internal bool UserIsZooming
        {
            get
            {
                return !m_needToValidate;
            }
        }

        public bool DefaultZoom
        {
            get
            {
                return (bool)Settings.DefaultZoom_Freq;
            }
            set
            {
                Settings.DefaultZoom_Freq = value;
            }
        }

        /// <summary>
        /// the zoom poition returned by sfCart is actually incorect if the view is close to the end of the graph
        /// </summary>
        public float ZoomPosition_Primary
        {
            get
            {
                ObservableCollection<Serializable_DataPoint> dataSeries = ((Series[Series.Count - 1] as FastLineSeries).ItemsSource as ObservableCollection<Serializable_DataPoint>);
                double dbMaxpoint = dataSeries[dataSeries.Count - 1].XVal;
                float flZoomWindowSec = (float)(PrimaryAxis.ZoomFactor * dbMaxpoint);

                float flZoomPosSec = (float)((PrimaryAxis.ZoomPosition * dbMaxpoint));

                // if the zoomPosition is within ZoomWindowSec of the endpoint of the series than return (dbMaxpoint - ZoomWindowSec)


                return (float)((dbMaxpoint - flZoomPosSec) < flZoomWindowSec ? (float)((dbMaxpoint - flZoomWindowSec) / dbMaxpoint) : PrimaryAxis.ZoomPosition);
            }
        }

        public float ZoomFactor_Primary
        {
            get
            {
                return (float) PrimaryAxis.ZoomFactor;
            }
            set
            {
                PrimaryAxis.ZoomFactor = value;
                Settings.ZoomXms_Freq = (int)(value * m_dbMaxXValOnChart * 1000f);
            }
        }

        public float ZoomFactor_Secondary
        {
            get
            {
                return (float) SecondaryAxis.ZoomFactor;
            }
            set
            {
                SecondaryAxis.ZoomFactor = value;
                Settings.ZoomY_Freq = value;
            }
        }

        public SfChartMainExt_Freq(Context context, ObservableCollection<ISerialisableDataPoint> lstPoints) : base(context)
        {
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            GetChildAt(0).SetOnTouchListener(this);

            Behaviors.CollectionChanged += Behaviors_CollectionChanged;

            base.ZoomStart += ZoomStartMethod;
            base.ZoomEnd += ZoomEndMethod;

            foreach (Serializable_DataPoint sd in lstPoints)
            {
                m_lstPitchPoints.Add(new Serializable_DataPoint(sd.XVal, ghFunc.LogValueForGraph_Get(sd.YVal), sd.PctCloseness, sd.Note));
            }

            m_parentActivity = (Activity)context;
            GraphTitle_Set();
            _maxFreqVal = ghFunc.LogValueForGraph_Get(NotePitchMap.PITCH_HIGH_LIMIT);
            _minFreqValue = 0;
        }

        private void ZoomEndMethod(object sender, ZoomEndEventArgs e)
        {
            ZoomEndEventArgsCustom evt = new ZoomEndEventArgsCustom();

            evt.CurrentZoomFactor = (float) e.CurrentZoomFactor;
            evt.CurrentZoomPosition = (float)e.CurrentZoomPosition;
            evt.ChartAxis = e.ChartAxis;
            this.ZoomEnd(sender, evt);
        }

        private void ZoomStartMethod(object obj, SfChart.ZoomStartEventArgs evt)
        {
            // Toast.MakeText(this, "ZoomStart Called", ToastLength.Short);
            this.ZoomStart(obj, evt);
        }

        public void ZoomToPoint( double x, double y)
        {
            int intOffsetYonGraph;
            double newZPosOnSecondary;
            double newZPosOnPrimary;
            double widthInDbOfZoomFactor, dbOffset;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            intOffsetYonGraph = ghFunc.OffsetForGraph_Get();            

            if (!UserIsZooming)
            {
                (Behaviors[0] as ChartZoomPanBehavior).ZoomMode = ZoomMode.Xy;
            }            

            if ((bool)Settings.DefaultZoom_Freq)
            {
                if (Math.Abs(y - _dblLastYpositionOnGraph) > (intOffsetYonGraph * Y_DIFFERENCE_GRAPH_THRESHOLD))
                {
                    // this prevent the graph jumping up and down too much in the vertical plane
                    _dblLastYpositionOnGraph = y;
                }
                DefaultZoom_Do(x , _dblLastYpositionOnGraph);             
            }
            else
            {
                PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Freq / 1000f) / m_dbMaxXValOnChart;
                SecondaryAxis.ZoomFactor = Settings.ZoomY_Freq ;
                widthInDbOfZoomFactor = Math.Abs(_maxFreqVal - _minFreqValue) * SecondaryAxis.ZoomFactor;
                dbOffset = (widthInDbOfZoomFactor / 2) * 0.9f;
                if (Math.Abs(y - _dblLastYpositionOnGraph) > (dbOffset))
                {
                    _dblLastYpositionOnGraph = y;
                }
                newZPosOnPrimary = Zoom_GetZommPostionToCenterTimeValueOnPrimaryAxisForZoomFactor((float)x, (float)PrimaryAxis.ZoomFactor);
                newZPosOnSecondary = Zoom_GetZommPostionToCenterFreqValueOnSecondaryAxisForZoomFactor((float)_dblLastYpositionOnGraph, (float)SecondaryAxis.ZoomFactor);

                (Behaviors[0] as ChartZoomPanBehavior).ZoomToFactor(PrimaryAxis, (float)newZPosOnPrimary, PrimaryAxis.ZoomFactor);

                if ((bool)Settings.SnapToNote_Freq)
                {
                    (Behaviors[0] as ChartZoomPanBehavior).ZoomToFactor(SecondaryAxis, (float)newZPosOnSecondary, SecondaryAxis.ZoomFactor);
                }
            }

            Invalidate();
        }

        private void DefaultZoom_Do(double x, double y)
        {
            int intOffsetYonGraph;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            intOffsetYonGraph = ghFunc.OffsetForGraph_Get();

            double xStart, xEnd, yStart, yEnd;
            xStart = x - ZOOM_OFFSET_X;

            xEnd = x + ZOOM_OFFSET_X;

            if (xStart < 0)
            {
                xStart = 0;
            }

            yStart = y - intOffsetYonGraph;
            yEnd = y + intOffsetYonGraph;

            if (yStart < 0)
            {
                yStart = 0;
            }

            (Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(PrimaryAxis, xStart, xEnd);

            if ((bool)Settings.SnapToNote_Freq)
            {
                (Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(SecondaryAxis, yStart, yEnd);
            }
        }

        public void DrawVerticalLineAtXpoint(double dblXpt)
        {
            ObservableCollection<Serializable_DataPoint> lsDp = new ObservableCollection<Serializable_DataPoint>();
            ObservableCollection<Serializable_DataPoint> lsDp_ChartOverview = new ObservableCollection<Serializable_DataPoint>();

            lsDp.Add(new Serializable_DataPoint(dblXpt, MAX_Y_VAL));
            lsDp.Add(new Serializable_DataPoint(dblXpt, MIN_Y_VAL));
            (Series[0] as FastLineSeries).ItemsSource = lsDp;

            Invalidate();
        }

        private double Zoom_GetZommPostionToCenterFreqValueOnSecondaryAxisForZoomFactor(float yVal, float zFact)
        {
            double widthInFreqOfZoomFactor = (_maxFreqVal - _minFreqValue) * zFact;

            double startFreq = yVal - widthInFreqOfZoomFactor / 2;

            if (startFreq < 0)
            {
                startFreq = 0;
            }
            return Zoom_GetPositionOnSecondaryAxisForFreqValue(startFreq);
        }

        public void RemoveLineSeries()
        {
            if (Series[INDEX_OF_VERTICAL_LINE_MAIN_CHART].ItemsSource != null)
            {
                (Series[INDEX_OF_VERTICAL_LINE_MAIN_CHART].ItemsSource as ObservableCollection<Serializable_DataPoint>).Clear();
            }
        }

        private double Zoom_GetPositionOnSecondaryAxisForFreqValue(double yVal)
        {
            return yVal / (_maxFreqVal - _minFreqValue);
        }

        private double Zoom_GetPositionOnPrimaryAxisFortimeValue(double xVal)
        {

            return xVal / m_dbMaxXValOnChart;
        }

        private double Zoom_GetZommPostionToCenterTimeValueOnPrimaryAxisForZoomFactor(float xVal, float zFact)
        {

            double widthInTimeOfZoomFactor = m_dbMaxXValOnChart * zFact;

            double starttime = xVal - widthInTimeOfZoomFactor / 2;

            if (starttime < 0)
            {
                starttime = 0;
            }
            return Zoom_GetPositionOnPrimaryAxisFortimeValue(starttime);
        }

        private void GraphTitle_Set()
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            Title.Text =  Common.Settings.TuningSystemAndRootScale + " : " + Common.Settings.A4ref + "Hz : " + Common.Settings.Transpose;
        }

        public void ReLoadSettings()
        {
            // remove note markers
            for (int i = INDEX_OF_FIRST_NM_ON_CHART; i <= m_intEndIndex; i++)
            {
                Series.RemoveAt(INDEX_OF_FIRST_NM_ON_CHART);
            }

            // re- Add markers

            AddNoteMarkersToChart(m_lstPitchPoints);            
            CalculateMaxXval(m_lstPitchPoints);
            GraphTitle_Set();
        }       


        private void CalculateMaxXval(ObservableCollection<ISerialisableDataPoint> lstPoints)
        {
            int numPoints;
            double maxXVal;
            numPoints = lstPoints.Count;
            maxXVal = lstPoints[numPoints - 1].XVal;
            m_dbMaxXValOnChart = (float)maxXVal;
        }

        private void Behaviors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                ChartBehavior behavior = e.NewItems[0] as ChartBehavior;

                if (behavior is ChartZoomPanBehavior)
                {
                    chartZoomPanBehavior = behavior as ChartZoomPanBehavior;
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                ChartBehavior behavior = e.OldItems[0] as ChartBehavior;

                if (behavior is ChartZoomPanBehavior)
                {
                    chartZoomPanBehavior = null;
                }
            }
        }

        internal List<YaxisMarker> GetNoteMakersInRange(double dblVisibleRangeStart, double dblVisibleRangeEnd)
        {
            List<YaxisMarker> lstNtMarkers = new List<YaxisMarker>();
            double dbFreq, diff;
            int intFreq;
            string strVal;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();

            foreach (FastLineSeries ls in m_lstFastLineSeriesNoteMarkers)
            {
                // m_pltView.Model.Series.Add(ls);
                double yVal = (ls.ItemsSource as ObservableCollection<Serializable_DataPoint>)[0].YVal;

                if ((yVal >= dblVisibleRangeStart) || (yVal <= dblVisibleRangeEnd))
                {
                    dbFreq = ghFunc.InverseLogValueForGraph_Get(yVal);

                    diff = dbFreq - Math.Round(dbFreq);
                    intFreq = (int)dbFreq;
                    // intFreq = ((dbFreq % 1) == 0) ? intFreq + 1 : intFreq;

                    strVal = NotePitchMap.GetNoteNameFromFreqency(intFreq);
                    lstNtMarkers.Add(new YaxisMarker(yVal, strVal));
                }
            }
            return lstNtMarkers;
        }

        public void HighLightClosestMarkerOnYAxis(double dblYcoord)
        {
            double dblPrctCloseness = 0;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();

            string strNoteName = NotePitchMap.GetNoteFromPitch(ghFunc.InverseLogValueForGraph_Get(dblYcoord), ref dblPrctCloseness);

            if (strNoteName != "")
            {

                (SecondaryAxis as NumericalAxisExt).NoteToHightlight = strNoteName;
            }

            Invalidate();
        }

        private void AddNoteMarkersToChart(ObservableCollection<ISerialisableDataPoint> lsDp)
        {
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();

            m_lstFastLineSeriesNoteMarkers.Clear();
            try
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In AddNoteMarkersToChart in pitch helper activity: numober of points in passed in object is : " + lsDp.Count);
#endif

                if (lsDp.Count <= 0)
                {
                    return;
                }
                m_lstFastLineSeriesNoteMarkers = GetPointofHorizontalNoteMarkers(lsDp[lsDp.Count - 1].XVal,
                                                                                    NotePitchMap.PITCH_LOW_LIMIT, 
                                                                                    NotePitchMap.PITCH_HIGH_LIMIT);


#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In AddNoteMarkersToChart : number of lines in m_lstFastLineSeriesNoteMarkers : " + m_lstFastLineSeriesNoteMarkers.Count);
#endif

                m_intStartIndex = Series.Count;
                foreach (FastLineSeries ls in m_lstFastLineSeriesNoteMarkers)
                {
                    // m_pltView.Model.Series.Add(ls);

                    Series.Add(ls);

                }

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In AddNoteMarkersToChart : number of lines in Series : " + Series.Count);
#endif

                m_intEndIndex = Series.Count - 1;
            }
            catch (Exception e1)
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In AddNoteMarkersToChart in pitch helper activity: exception  : " + e1.Message);
#endif
                throw e1;
            }
        }

        private bool CoordInRange(float val1, float val2, int viewWidth)
        {
            float flAllowableDeviation = viewWidth * GRAPH_TAP_ALLOWABLE_PERCENT_ERROR;
            if (Math.Abs(val1 - val2) <= flAllowableDeviation)
            {
                return true;
            }
            return false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            //If the Independent axis zoom is not enabled, ignore

            if (e.Action == MotionEventActions.Up)
            {
                m_needToValidate = true;
            }
            int viewWidth = v.Right - v.Left;

            if (e.PointerCount == 1 && chartZoomPanBehavior != null)
            {
                int pointer1Index = e.FindPointerIndex(0);

                if (pointer1Index == -1)
                {
                    return base.OnTouchEvent(e);
                }
                if (e.Action == MotionEventActions.Down)
                {

                    m_x1 = e.GetX(pointer1Index);
                    m_y1 = e.GetY(pointer1Index);
                }
                else if (e.Action == MotionEventActions.Up)
                {
                    float x1 = e.GetX(pointer1Index);
                    float y1 = e.GetY(pointer1Index);

                    if (e.EventTime - e.DownTime > 500)
                    {
                        // this was probably intended as a long press => do nothing
                        return base.OnTouchEvent(e);
                    }

                    if (CoordInRange(x1, m_x1, viewWidth) && CoordInRange(m_y1, y1, viewWidth))
                    {
                        GraphClick(this, new ChartEventArgs.GraphClickedEventArgs(v, x1));
                    }
                }
            }
            
            if (e.PointerCount == 2 && m_needToValidate && chartZoomPanBehavior != null)
            {

                int pointer1Index = e.FindPointerIndex(0);
                int pointer2Index = e.FindPointerIndex(1);

                float x1 = e.GetX(pointer1Index);
                float y1 = e.GetY(pointer1Index);

                float x2 = e.GetX(pointer2Index);
                float y2 = e.GetY(pointer2Index);

                double radians = Math.Atan2(x2 - x1, -(y2 - y1));

                if (radians < 0)
                    radians = Math.Abs(radians);
                else
                    radians = 2 * Math.PI - radians;

                var angle = radians * (180 / Math.PI);

                //Diagonal zooming
                if ((angle > 20 && angle < 70) || (angle > 110 && angle < 160) || (angle > 200 && angle < 250) || (angle > 290 && angle < 340))
                {
                    chartZoomPanBehavior.ZoomMode = ZoomMode.Xy;

                    if (chartZoomPanBehavior != null)
                        chartZoomPanBehavior.ZoomMode = ZoomMode.Xy;

                }
                else if (Math.Abs(x1 - x2) > Math.Abs(y1 - y2))
                {
                    chartZoomPanBehavior.ZoomMode = ZoomMode.X;

                    if (chartZoomPanBehavior != null)
                        chartZoomPanBehavior.ZoomMode = ZoomMode.X;

                }
                else if (Math.Abs(y1 - y2) > Math.Abs(x1 - x2))
                {
                    chartZoomPanBehavior.ZoomMode = ZoomMode.Y;

                    if (chartZoomPanBehavior != null)
                        chartZoomPanBehavior.ZoomMode = ZoomMode.Y;
                }
                m_needToValidate = false;
            }

            return base.OnTouchEvent(e);
        }

        internal void GraphLongClick(object obj, EventArgs evt)
        {
            const int VERT_ZOOM_MULT_FACT = 8;
            var dialog = PopUpForLongGraphClick.NewInstance((bool)Settings.SnapToNote_Freq,
                                                    (bool)Settings.DefaultZoom_Freq,
                                                    Settings.ZoomXms_Freq,
                                                    Math.Min( 1f, Settings.ZoomY_Freq * VERT_ZOOM_MULT_FACT),
                                                    m_parentActivity);
            dialog.SnapToNoteSwitchChanged += (s, b) =>
            {
                Settings.SnapToNote_Freq = b;
            };

            dialog.DefaultZoomSwitchChanged += (s, b) =>
            {
                Settings.DefaultZoom_Freq = b;
                if (!b)
                {
                    PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Freq / 1000f) / m_dbMaxXValOnChart;
                    SecondaryAxis.ZoomFactor = Settings.ZoomY_Freq;
                }
                Invalidate();
            };

            dialog.ZoomXChanged += (s, b) =>
            {
                ZoomEndEventArgsCustom args = new ZoomEndEventArgsCustom();
                Settings.ZoomXms_Freq = b;
                Settings.ZoomXms_Db = b;
                args.ChartAxis = PrimaryAxis;
                args.CurrentZoomFactor = (b / 1000f) / m_dbMaxXValOnChart;
                args.CurrentZoomPosition = (float) PrimaryAxis.ZoomPosition;
                // for now both graph have the same zoom x value
                ZoomEnd?.Invoke(this, args);
                Invalidate();
            };

            dialog.ZoomYChanged += (s, b) =>
            {
                Settings.ZoomY_Freq = Math.Max(0.02f, b / VERT_ZOOM_MULT_FACT);
                SecondaryAxis.ZoomFactor = Settings.ZoomY_Freq;
                Invalidate();
            };

            dialog.Show(m_parentActivity.FragmentManager, "dialog");
        }


        internal static ObservableCollection<FastLineSeries> GetPointofHorizontalNoteMarkers(double dblXMaxVal, double dblYminVal, double dblYmaxVal)
        {
            ObservableCollection<FastLineSeries> lsFastLineSeries = new ObservableCollection<FastLineSeries>();

            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct0, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct1, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct2, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct3, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct4, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct5, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct6, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct7, dblXMaxVal, dblYminVal, dblYmaxVal);
            CreateLinePoints(ref lsFastLineSeries, NotePitchMap.oct8, dblXMaxVal, dblYminVal, dblYmaxVal);
            return lsFastLineSeries;
        }

        private static void CreateLinePoints(ref ObservableCollection<FastLineSeries> lsDataPoints, double[] oct, double dblXMaxVal, double dblYminVal, double dblYmaxVal)
        {
            ObservableCollection<Serializable_DataPoint> lsDP;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            for (int i = 0; i < NUM_NOTES_PER_OCTAVE; i++)
            {

                if ((oct[i] < dblYminVal) || (oct[i] > dblYmaxVal))
                {
                    continue;
                }
                FastLineSeries ls;

                ls = new FastLineSeries
                {
                    //  Color = Color.ParseColor("#9fff80"),
                    Color = Color.Green,
                    Alpha = 0.5f,
                    XBindingPath = "XVal",
                    YBindingPath = "YVal"
                };

                lsDP = new ObservableCollection<Serializable_DataPoint>();
                for (int j = 0; j < (int)dblXMaxVal; j += 5)
                {
                    // ls.Points.Add(new DataPoint(j, oct[i]));
                    lsDP.Add(new Serializable_DataPoint(j, ghFunc.LogValueForGraph_Get(oct[i])));
                }

                // lsDP.RemoveAt(lsDP.Count - 1);
                lsDP.Add(new Serializable_DataPoint(dblXMaxVal, ghFunc.LogValueForGraph_Get(oct[i])));
                ls.ItemsSource = lsDP;
                lsDataPoints.Add(ls);
            }
        }

        public void ClearAllSeries()
        {
            Series.Clear();
        }

        public void CleanUp()
        {
            base.ZoomStart -= ZoomStartMethod;

            Behaviors.CollectionChanged -= Behaviors_CollectionChanged;

            ClearAllSeries();

            base.Dispose();
        }
    }
}