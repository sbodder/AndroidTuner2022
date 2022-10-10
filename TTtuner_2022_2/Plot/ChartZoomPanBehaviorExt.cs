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
using Com.Syncfusion.Charts;
using TTtuner_2022_2.EventHandlersTidy;
using TTtuner_2022_2.PopUpDialogFragments;

namespace TTtuner_2022_2.Plot
{
    internal class ChartZoomPanBehaviorExt : ChartZoomPanBehavior, IDisposable
    {
        internal event EventHandler LongPress;
        
        
        internal ChartZoomPanBehaviorExt()
        {
            DoubleTapEnabled = false;
        }

        protected override void OnLongPress(float pointX, float pointY)
        {
            LongPress(this, new EventArgs());

            base.OnLongPress(pointX, pointY);
        }


        protected override void OnDoubleTap(float pointX, float pointY)
        {
            // wish to cancel this event so don't call base
        }

        public void Dispose()
        {
            cEventHelper.RemoveAllEventHandlers(LongPress);
        }



        protected override void OnTouchDown(float pointX, float pointY)
        {
            // I was thinking of using this event so that if the user touches the grpah the play marker would move to that spot.
            // This difficulty is that this event is also fired when the user does a pinch gestreu to zoom. Would have to distuingish between them
            // for it to work.
            // could do something like store the coords and time of touch down
            // overide touch up as well and if the coords of touch up match as well as the time then its a graph click

            // there is also the issue of relating the coords to an acutal time on the track. You will need to find how much the view 
            //  is zoomed by for this to work. Can get this by saving the zoom factor in the ZoomStartMethod in PitchPlotHelper.
            // use a meber deleage in ChartZoomPanBehaviorExt to call PitchPlotHelper for this value of the zoom factor           

            base.OnTouchDown(pointX, pointY);
        }



    }
}