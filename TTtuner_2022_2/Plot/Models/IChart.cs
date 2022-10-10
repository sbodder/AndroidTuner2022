using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Com.Syncfusion.Charts;
using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using static Com.Syncfusion.Charts.SfChart;

namespace TTtuner_2022_2.Plot
{
    interface IChart
    {
        event EventHandler<View.TouchEventArgs> Touch;
        event EventHandler<ZoomStartEventArgs> ZoomStart;
        event EventHandler<ZoomEndEventArgsCustom> ZoomEnd;
        event EventHandler<ChartEventArgs.GraphClickedEventArgs> GraphClick;
        ObservableCollection<ISerialisableDataPoint> DataPoints { get; }
        float ZoomPosition_Primary { get; }
        float ZoomFactor_Primary { get; set; }
        float ZoomFactor_Secondary { get; set; }

        float TimeWindowStart { get; }

        bool DefaultZoom { get; set; }

        void ReLoadSettings();

        void HighLightClosestMarkerOnYAxis(double Y);

        void ZoomToPoint( double x, double y);

        void DrawVerticalLineAtXpoint(double dblXpt);

        void RemoveLineSeries();

        View View { get; }

        void ClearAllSeries();

        void CleanUp();

    }


    public class ZoomEndEventArgsCustom 
    {
        public ChartAxis ChartAxis { get; set; }
        public float CurrentZoomFactor { get; set; }
        public float CurrentZoomPosition { get; set; }
        
    }
}