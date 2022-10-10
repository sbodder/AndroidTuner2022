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
using Com.Syncfusion.Charts;
using TTtuner_2022_2.Plot;

namespace TTtuner_2022_2.Fragments
{
    class ScatterPlotFragment : AndroidX.Fragment.App.Fragment
    {
        ChartZoomPanBehavior _zoomPanBehavior;
        SfChart _chart;
        private Activity m_act;
        private View m_view;
        private ObservableCollection<ISerialisableDataPoint> _dataseries;
        const double _TIME_WINDOW_WIDTH = 3;

        public bool SetupComplete { get; set; } = false;

        public ScatterPlotFragment()
        {

        }
        public static ScatterPlotFragment NewInstance(Activity act)
        {
            var scatterFrag = new ScatterPlotFragment
            {
                Arguments = new Bundle(),
                m_act = act,
                _dataseries = new ObservableCollection<ISerialisableDataPoint>(),
            };

            return scatterFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            m_view = inflater.Inflate(Resource.Layout.ScatterPlotFragment, container, false);
            SetupChart();
            SetupComplete = true;
            return m_view;
        }

        public void AddPointToChart(Serializable_DataPoint dp)
        {
            //_chart.SuspendSeriesNotification();
            _dataseries.Add(dp.Clone() as Serializable_DataPoint);

            foreach (var item in _dataseries.ToList())
            {
                if (item.XVal < dp.XVal - _TIME_WINDOW_WIDTH)
                {
                    _dataseries.Remove(item);
                }
            }            
            _zoomPanBehavior.ZoomByRange(_chart.PrimaryAxis, dp.XVal - _TIME_WINDOW_WIDTH, dp.XVal );
        }

        internal void ClearPoints()
        {
            _dataseries.Clear();
        }

        private void SetupChart()
        {
            LinearLayout llay = (LinearLayout)m_view.FindViewById(Resource.Id.LinearLayoutForScatterPlot);

            _chart = new SfChart(m_act);
            _chart.Title.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 13f);
            _chart.Title.SetTextColor(Color.Rgb(0x99, 0xcc, 0));
            _chart.SetBackgroundColor(Color.Black);

            TimeAxisForScatterFrag primaryAxis = new TimeAxisForScatterFrag();
            primaryAxis.Minimum = - _TIME_WINDOW_WIDTH;
            _chart.PrimaryAxis = primaryAxis;
            primaryAxis.ShowMinorGridLines = false;
            primaryAxis.ShowMajorGridLines = false;
            primaryAxis.Maximum = 100000;
            primaryAxis.Title.TextColor = Color.Rgb(0x99, 0xcc, 0);
            primaryAxis.LabelStyle.TextColor = Color.Rgb(0x99, 0xcc, 0);

            CentsAxisForScatterFrag secondaryAxis = new CentsAxisForScatterFrag();
            secondaryAxis.Maximum = 50;
            secondaryAxis.Minimum = -50;
            secondaryAxis.LabelStyle.TextColor = Color.Rgb(0x99, 0xcc, 0);
            secondaryAxis.ShowMinorGridLines = false;
            secondaryAxis.ShowMajorGridLines = false;
            secondaryAxis.LabelStyle.TextSize = 8;
            secondaryAxis.StripLines.Add(SetupStripline(25, Color.Red));
            secondaryAxis.StripLines.Add(SetupStripline(0, Color.Rgb(0x99, 0xcc, 0)));
            secondaryAxis.StripLines.Add(SetupStripline(-25, Color.Red));

            _zoomPanBehavior = new ChartZoomPanBehavior();
            _zoomPanBehavior.SelectionZoomingEnabled = false;
            _zoomPanBehavior.ZoomingEnabled = false;
            _chart.Behaviors.Add(_zoomPanBehavior);

            _chart.SecondaryAxis = secondaryAxis;

            FastScatterSeries fastScatterSeries = new FastScatterSeries()
            {
                ScatterHeight = 3,
                ScatterWidth = 3,
                ShapeType = ChartScatterShapeType.Ellipse,
                ItemsSource = _dataseries,
                XBindingPath = "XVal",
                YBindingPath = "CentsDeviation",
                Color = Color.White,
            };

            _chart.Series.Add(fastScatterSeries);

            llay.AddView(_chart);
        }

        private NumericalStripLine SetupStripline(int markerVal, Color cl)
        {
            NumericalStripLine numericalStripLine = new NumericalStripLine();
            numericalStripLine.Start = markerVal;
            numericalStripLine.Width = 1;
            numericalStripLine.FillColor = cl;
            return numericalStripLine;
        }
    }
}