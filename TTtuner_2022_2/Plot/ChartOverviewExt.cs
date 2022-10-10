using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using Com.Syncfusion.Charts;

namespace TTtuner_2022_2.Plot
{
    class SfChartOverviewExt : SfChart, IChart
    {
        ObservableCollection<ISerialisableDataPoint> m_lstPitchPoints;
        internal float m_flMaxTimeInDataSeries;
        internal double m_MinYvalOnChartOverview;
        internal double m_MaxYvalOnChartOverview;
        internal const int INDEX_OF_DATA_SERIES_ON_CHART_OVERVIEW = 0;
        internal const int INDEX_OF_VERTICAL_LINE_SERIES_ON_CHART_OVERVIEW = 1;
        public event EventHandler<ZoomEndEventArgsCustom> ZoomEnd;

        public event EventHandler<ChartEventArgs.GraphClickedEventArgs> GraphClick;

        public ObservableCollection<ISerialisableDataPoint> DataPoints
        {
            get
            {
                return m_lstPitchPoints;
            }
        }

        public bool DefaultZoom  {  get; set; }


        public event EventHandler<View.TouchEventArgs> Touch;

        public SfChartOverviewExt(Context context, ObservableCollection<ISerialisableDataPoint> lstPitchPoints) : base(context)
        {
            m_lstPitchPoints = lstPitchPoints;

            base.Touch += GraphOverviewClicked;
        }

        public float ZoomPosition_Primary { get;  }
        public float ZoomFactor_Primary { get; set; }
        public float ZoomFactor_Secondary { get; set; }
        public View View
        {
            get
            {
                return this;
            }
        }
        public float TimeWindowStart
        {
            get
            {
                ObservableCollection<ISerialisableDataPoint> dataSeries = ((Series[INDEX_OF_DATA_SERIES_ON_CHART_OVERVIEW] as FastLineSeries).ItemsSource as ObservableCollection<ISerialisableDataPoint>);
                ObservableCollection<ISerialisableDataPoint> verticalLineSeries = ((Series[INDEX_OF_VERTICAL_LINE_SERIES_ON_CHART_OVERVIEW] as FastLineSeries).ItemsSource as ObservableCollection<ISerialisableDataPoint>);


                double minXpt = dataSeries[0].XVal;
                double maxXpt = dataSeries[dataSeries.Count - 1].XVal;
                // this is a quick of how this graph is diplayed. IF the vertical line is diplayed after the min point any gap between
                // 0  and the min point will not be displayed. other wise it will
                if (verticalLineSeries[0].XVal > minXpt)
                {
                    return (float)(minXpt);
                }
                else
                {
                    return 0f;
                }
            }
        }

        private void GraphOverviewClicked(object obj, View.TouchEventArgs evt)
        {
            Touch(obj, evt);
        }

        public void DrawVerticalLineAtXpoint(double dblXpt)
        {
            ObservableCollection<ISerialisableDataPoint> lsDp = new ObservableCollection<ISerialisableDataPoint>();
            ObservableCollection<ISerialisableDataPoint> lsDp_ChartOverview = new ObservableCollection<ISerialisableDataPoint>();
            
            
            lsDp_ChartOverview.Add(new Serializable_DataPoint(dblXpt, m_MaxYvalOnChartOverview));
            lsDp_ChartOverview.Add(new Serializable_DataPoint(dblXpt, m_MinYvalOnChartOverview));

            (Series[INDEX_OF_VERTICAL_LINE_SERIES_ON_CHART_OVERVIEW] as FastLineSeries).ItemsSource = lsDp_ChartOverview;

            Invalidate();

        }

        public void ClearAllSeries()
        {
            Series.Clear();
        }

        internal double GetMinYVAlueInList(ObservableCollection<ISerialisableDataPoint> lstPitchPoints)
        {
            var datapoint = lstPitchPoints
                .OrderBy(p => p.YVal)
                .FirstOrDefault();

            return datapoint.YVal;
        }

        internal double GetMaxYVAlueInList(ObservableCollection<ISerialisableDataPoint> lstPitchPoints)
        {
            var datapoint = lstPitchPoints
                .OrderByDescending(p => p.YVal)
                .FirstOrDefault();

            return datapoint.YVal;
        }

        public void RemoveLineSeries()
        {
            if (Series[INDEX_OF_VERTICAL_LINE_SERIES_ON_CHART_OVERVIEW].ItemsSource != null)
            {
                (Series[INDEX_OF_VERTICAL_LINE_SERIES_ON_CHART_OVERVIEW].ItemsSource as ObservableCollection<ISerialisableDataPoint>).Clear();
            }
        }



        public void ReLoadSettings()
        {
            
        }

        public void HighLightClosestMarkerOnYAxis(double dblYcoord)
        {
           
        }

        public void ZoomToPoint(double x, double y)
        {

        }

        public void CleanUp()
        {
            base.Touch -= GraphOverviewClicked;

            base.Dispose();
        }




    }

}