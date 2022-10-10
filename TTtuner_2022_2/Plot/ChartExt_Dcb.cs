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
    internal class SfChartMainExt_Dcb : SfChart, View.IOnTouchListener, IChart
    {
        const int INDEX_OF_DS_ON_CHART = 1;
        const int INDEX_OF_VL_ON_CHART = 0;
        const int INDEX_OF_FIRST_NM_ON_CHART = 2;
        const int INDEX_OF_VERTICAL_LINE_MAIN_CHART = 0;

        double _dblLastYpositionOnGraph = double.MaxValue;
        const double ZOOM_OFFSET_X = 0.75d;
        const double ZOOM_OFFSET_Y = 0.12d;

        private const int NUM_NOTES_PER_OCTAVE = 12;
        double _intOffsetYonGraph;
        internal double _maxY = 0;
        internal double _minY;
        bool m_needToValidate = true;
        ObservableCollection<FastLineSeries> m_lstFastLineSeriesNoteMarkers = new ObservableCollection<FastLineSeries>();
        ObservableCollection<ISerialisableDataPoint> m_lstDataPoints;
        private Activity m_parentActivity;

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

        public ObservableCollection<ISerialisableDataPoint> DataPoints
        {
            get
            {
                return m_lstDataPoints;
            }
        }

        public bool DefaultZoom
        {
            get
            {
                return (bool)Settings.DefaultZoom_Db;
            }
            set
            {
                Settings.DefaultZoom_Db = value;
            }
        }

        public View View
        {
            get
            {
                return this;
            }
        }

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

        internal bool UserIsZooming
        {
            get
            {
                return !m_needToValidate;
            }
        }

        /// <summary>
        /// the zoom poition returned by sfCart is actually incorect if the view is close to the end of the graph
        /// </summary>
        public float ZoomPosition_Primary
        {
            get
            {
                ObservableCollection<Serializable_DataPoint_Std> dataSeries = ((Series[Series.Count - 1] as FastLineSeries).ItemsSource as ObservableCollection<Serializable_DataPoint_Std>);
                double dbMaxpoint = dataSeries[dataSeries.Count - 1].XVal;
                float flZoomWindowSec = (float)(PrimaryAxis.ZoomFactor * dbMaxpoint);

                float flZoomPosSec = (float)((PrimaryAxis.ZoomPosition * dbMaxpoint));
                return (float) ((dbMaxpoint - flZoomPosSec) < flZoomWindowSec ? (float)((dbMaxpoint - flZoomWindowSec) / dbMaxpoint) : PrimaryAxis.ZoomPosition);
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
                Settings.ZoomXms_Db = (int)(value * m_dbMaxXValOnChart * 1000f);
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
                Settings.ZoomY_Db = value;
            }
        }

        public SfChartMainExt_Dcb(Context context, ObservableCollection<ISerialisableDataPoint> lstPoints) : base(context)
        {
            GetChildAt(0).SetOnTouchListener(this);

            Behaviors.CollectionChanged += Behaviors_CollectionChanged;

            base.ZoomStart += ZoomStartMethod;

            base.ZoomEnd += ZoomEndMethod;
            m_lstDataPoints = lstPoints;

            m_parentActivity = (Activity)context;

            GraphTitle_Set();
            _minY = GetMinYVAlueInList();
           
        }

        private void ZoomEndMethod(object sender, ZoomEndEventArgs e)
        {
            ZoomEndEventArgsCustom evt = new ZoomEndEventArgsCustom();

            evt.CurrentZoomFactor = (float) e.CurrentZoomFactor;
            evt.CurrentZoomPosition = (float) e.CurrentZoomPosition;
            evt.ChartAxis = e.ChartAxis;
            this.ZoomEnd(sender, evt);
        }

        internal double GetMinYVAlueInList()
        {
            var datapoint = m_lstDataPoints
                .OrderBy(p => p.YVal)
                .FirstOrDefault();

            return datapoint.YVal;
        }

        private void ZoomStartMethod(object obj, SfChart.ZoomStartEventArgs evt)
        {
            // Toast.MakeText(this, "ZoomStart Called", ToastLength.Short);
            this.ZoomStart(obj, evt);


        }

        public void ZoomToPoint(double x, double y)
        {
            double newZPosOnSecondary;
            double newZPosOnPrimary;
            double widthInDbOfZoomFactor;
            if (!UserIsZooming)
            {
                (Behaviors[0] as ChartZoomPanBehavior).ZoomMode = ZoomMode.Xy;
            }

            if ((bool)Settings.DefaultZoom_Db)
            {
                _intOffsetYonGraph = Math.Abs(_maxY - _minY) * 0.15f;
                if (Math.Abs(y - _dblLastYpositionOnGraph) > (_intOffsetYonGraph))
                {
                    _dblLastYpositionOnGraph = y;
                }
                DefaultZoom_Do(x, _dblLastYpositionOnGraph);
            }
            else
            {
                // for the ZoomToFactor function , the zoom position and the zoom factor should both between 0 and 1.
                // it doen't matter if the values are negative. What this function does is take the min and max value on the given axis
                // it take the zoom positon and zoom to that point using the zoom factor                
                PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Db / 1000f) / m_dbMaxXValOnChart;
                SecondaryAxis.ZoomFactor = Settings.ZoomY_Db;
                widthInDbOfZoomFactor = Math.Abs(_maxY - _minY) * SecondaryAxis.ZoomFactor;
                _intOffsetYonGraph = (widthInDbOfZoomFactor / 2) * 0.8f ;
                if (Math.Abs(y - _dblLastYpositionOnGraph) > (_intOffsetYonGraph))
                {
                    _dblLastYpositionOnGraph = y;
                }

                newZPosOnPrimary = Zoom_GetZommPostionToCenterTimeValueOnPrimaryAxisForZoomFactor((float)x, (float) PrimaryAxis.ZoomFactor);
                newZPosOnSecondary = Zoom_GetZommPostionToCenterFreqValueOnSecondaryAxisForZoomFactor((float)_dblLastYpositionOnGraph, (float)SecondaryAxis.ZoomFactor);

                (Behaviors[0] as ChartZoomPanBehavior).ZoomToFactor(PrimaryAxis, (float)newZPosOnPrimary, PrimaryAxis.ZoomFactor);

                if ((bool)Settings.SnapToNote_Db)
                {
                    (Behaviors[0] as ChartZoomPanBehavior).ZoomToFactor(SecondaryAxis, (float)newZPosOnSecondary, SecondaryAxis.ZoomFactor);
                }
            }

            Invalidate();
        }

        private void DefaultZoom_Do(double x, double y)
        {
            double xStart, xEnd, yStart, yEnd;

            xStart = x - ZOOM_OFFSET_X;
            xEnd = x + ZOOM_OFFSET_X;
            if (xStart < 0)
            {
                xStart = 0;
            }

            yStart = y - _intOffsetYonGraph;
            yEnd = y + _intOffsetYonGraph;

            if (yEnd > 0)
            {
                yEnd = 0;
            }

            (Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(PrimaryAxis, xStart, xEnd);

            if ((bool)Settings.SnapToNote_Db)
            {
                (Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(SecondaryAxis, yStart, yEnd);
            }
        }

        public void DrawVerticalLineAtXpoint(double dblXpt)
        {
            ObservableCollection<Serializable_DataPoint_Std> lsDp = new ObservableCollection<Serializable_DataPoint_Std>();
            ObservableCollection<Serializable_DataPoint_Std> lsDp_ChartOverview = new ObservableCollection<Serializable_DataPoint_Std>();

            lsDp.Add(new Serializable_DataPoint_Std(dblXpt, _maxY));
            lsDp.Add(new Serializable_DataPoint_Std(dblXpt, _minY));
            (Series[0] as FastLineSeries).ItemsSource = lsDp;

            Invalidate();
        }



        private double Zoom_GetZommPostionToCenterFreqValueOnSecondaryAxisForZoomFactor(float yVal, float zFact)
        {
            double widthInDbOfZoomFactor = Math.Abs(_maxY - _minY) * zFact;
            double startDb = yVal - widthInDbOfZoomFactor / 2;
            return Zoom_GetPositionOnSecondaryAxisForDbValue(startDb);
        }

        public void RemoveLineSeries()
        {
            if (Series[INDEX_OF_VERTICAL_LINE_MAIN_CHART].ItemsSource != null)
            {
                (Series[INDEX_OF_VERTICAL_LINE_MAIN_CHART].ItemsSource as ObservableCollection<Serializable_DataPoint_Std>).Clear();
            }
        }


        private double Zoom_GetPositionOnSecondaryAxisForDbValue(double yVal)
        {
            return 1 - Math.Abs(yVal / _minY);
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
            Title.Text = "Decibel (dB) Full Scale ";

        }


        public void ReLoadSettings()
        {
            CalculateMaxXval(m_lstDataPoints);
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

        public void HighLightClosestMarkerOnYAxis(double dblYcoord)
        {
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
            var dialog = PopUpForLongGraphClick.NewInstance((bool)Settings.SnapToNote_Db,
                                                    (bool)Settings.DefaultZoom_Db,
                                                    Settings.ZoomXms_Db,
                                                    Settings.ZoomY_Db,
                                                    m_parentActivity);
            dialog.SnapToNoteSwitchChanged += (s, b) =>
            {
                Settings.SnapToNote_Db = b;
            };

            dialog.DefaultZoomSwitchChanged += (s, b) =>
            {
                Settings.DefaultZoom_Db = b;
                if (!b)
                {
                    PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Db / 1000f) / m_dbMaxXValOnChart;
                    SecondaryAxis.ZoomFactor = Settings.ZoomY_Db;
                }
            };

            dialog.ZoomXChanged += (s, b) =>
            {
                // for now both graph have the same zoom x value
                ZoomEndEventArgsCustom args = new ZoomEndEventArgsCustom();
                Settings.ZoomXms_Freq = b;
                Settings.ZoomXms_Db = b;
                args.ChartAxis = PrimaryAxis;
                args.CurrentZoomFactor = (b / 1000f) / m_dbMaxXValOnChart;
                args.CurrentZoomPosition = (float)PrimaryAxis.ZoomPosition;
                // for now both graph have the same zoom x value
                ZoomEnd?.Invoke(this, args);
            };

            dialog.ZoomYChanged += (s, b) =>
            {
                Settings.ZoomY_Db = Math.Max( 0.07f, b);
                SecondaryAxis.ZoomFactor = Settings.ZoomY_Db;
                Invalidate();
            };

            dialog.Show(m_parentActivity.FragmentManager, "dialog");
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