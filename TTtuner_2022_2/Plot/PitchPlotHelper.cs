using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Syncfusion.Charts;
using Android.App;

using System.Threading;
using System.IO;
using TTtuner_2022_2.DSP;
using TTtuner_2022_2.Music;
using System.Collections;
using Android.Media;
using Android.Graphics;
using System.Collections.ObjectModel;
using TTtuner_2022_2.Audio;
using System.Reflection;
using TTtuner_2022_2.EventHandlersTidy;
using global::Android.Widget;
using Android.Util;
using global::Android.Views;
using TTtuner_2022_2.PopUpDialogFragments;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Plot
{
    internal class PlotHelper : Activity
    {
        private IAudioPlayer _audPlayer = null;

        float _flMaxTimeInDataSeries;

        const int NUM_INIT_ZOOMS = 5;

        //this variable is a neccessary evil. I tried several ways to init the zoom when the view first appears but of them worked properly
        // this I had to resort to this hack.
        private int _intNumStartZooms = 0;

        internal class GraphUpdateEventArgs : EventArgs
        {
            internal int CurrrentPositionMS { get; private set; }

            internal GraphUpdateEventArgs(int intCurrrentPositionMS)
            {
                CurrrentPositionMS = intCurrrentPositionMS;
            }
        }

        public bool IsDestroyed { get; set; }


        public bool IsPlaying
        {
            get
            {
                if (_audPlayer != null)
                {
                    return _audPlayer.IsPlaying;
                }
                else
                {
                    return false;
                }
            }
        }

        internal delegate void GraphUpdateEventHandler(object sender, GraphUpdateEventArgs e);

        internal event GraphUpdateEventHandler GraphUpdated;

        internal delegate void GraphClickedEventHandler(object sender, EventArgs e);

        private System.Timers.Timer _Timer;

        //private static string _strLastNote = "";
        //private static double _dblLastPrctCloseness = 0.0;
        private Activity _parentActivity;

        IChart[] _sfChartArray;
        IChart _sfChartOverview;

        internal IChart[] SF_ArrayOfCharts { get { return (IChart[])_sfChartArray; } }
        internal View SF_ChartOverview { get { return (View)_sfChartOverview; } }

        internal int Duration
        {
            get
            {
                return _audPlayer.Duration;
            }
        }
        DateTime _dtTime;

        long _lngLastGraphTouchEventTime = 0;

        internal PlotHelper(Activity act, IAudioPlayer audPlayer, IChart[] chartMainArray, IChart chartOverview)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();

            _audPlayer = audPlayer;
            _parentActivity = act;
            _dtTime = new DateTime();

            _sfChartArray = chartMainArray;
            _sfChartOverview = chartOverview;

            _flMaxTimeInDataSeries = GetMaxXValInAllCharts();

            foreach (IChart ic in _sfChartArray)
            {
                ic.ZoomStart += ZoomStartMethod;
                ic.GraphClick += GraphClick;
                ic.ZoomEnd += ZoomEndMethod;
            }
            _sfChartOverview.Touch += GraphOverviewClicked;

            SetUpTimer(100);
            IsDestroyed = false;
        }

        private void ZoomEndMethod(object sender, ZoomEndEventArgsCustom e)
        {

            // if this is the time axis then set all the graphs to have the same zoom factor
            // as the evert arg the user just set
            if ((sender as SfChart).PrimaryAxis == e.ChartAxis)
            {
                foreach (IChart ic in _sfChartArray)
                {
                    ic.DefaultZoom = false;
                    if (ViewWindowSecondsGet(e.CurrentZoomFactor) > 0.010f)
                    {
                        ic.ZoomFactor_Primary = e.CurrentZoomFactor;
                    }
                }
            }
            else
            {
                (sender as IChart).DefaultZoom = false;
                (sender as IChart).ZoomFactor_Secondary = Math.Max(0.005f , e.CurrentZoomFactor);
            }
        }

        private float GetMaxXValInAllCharts()
        {
            float flMax = 0;
            foreach (IChart ic in _sfChartArray)
            {
                flMax = Math.Max(flMax, (float)ic.DataPoints[ic.DataPoints.Count - 1].XVal);
            }
            return flMax;
        }

        private void ZoomStartMethod(object obj, SfChart.ZoomStartEventArgs evt)
        {
            if (obj == _sfChartArray[0])
            {
                _sfChartArray[0].DefaultZoom = false;
            }
            else if (obj == _sfChartArray[1])
            {
                _sfChartArray[1].DefaultZoom = false;
            }
        }

        private void GraphOverviewClicked(object obj, View.TouchEventArgs evt)
        {
            int iTime, rTime;
            int iLeft;
            int iRight;
            float fTime;

            //check that this event has not been processed a few milli seconds ago
            // the evt.Event.EventTime holds the time in milliseconds;

            if (Math.Abs(_lngLastGraphTouchEventTime - evt.Event.EventTime) < 500)
            {
                return;
            }

            _lngLastGraphTouchEventTime = evt.Event.EventTime;
            iRight = (obj as View).Right;
            int width = (obj as View).Width;

            iLeft = (obj as View).Left;

            fTime = (evt.Event.RawX - iLeft) / (iRight - iLeft);
            // sometimes the graph overview can start at a non zero time when the first element in the data series doesn't begin at 0
            iTime = (int)(_sfChartOverview.TimeWindowStart * 1000 + (fTime * (_flMaxTimeInDataSeries - _sfChartOverview.TimeWindowStart) * 1000));

#if Release_LogOutput || DEBUG
            Logger.Info(Common.CommonFunctions.APP_NAME, "In GraphOverviewTouched in pitch helper activity: event time is " + evt.Event.EventTime);
            Logger.Info(Common.CommonFunctions.APP_NAME, "In GraphOverviewTouched in pitch helper activity: seek to time is " + iTime);
#endif
            SeekTo(iTime);
            GraphUpdated(this, new GraphUpdateEventArgs(iTime));
        }

        private static void AddEmptyStringsToList(ref List<string> lsLabels, int intNumOfEmptyStrings)
        {
            for (int i = 0; i < intNumOfEmptyStrings; i++)
            {
                lsLabels.Add("");
            }

        }

        internal void ReLoadSettings()
        {
            int numPoints;
            double maxXVal;

            foreach (IChart ic in _sfChartArray)
            {
                ic.ReLoadSettings();
            }
        }

        internal void ZoomToPointAndHighLightNote(double x, double y, int indexOfChart)
        {
            if (_sfChartArray != null)
            {
                _sfChartArray[indexOfChart].ZoomToPoint(x, y);
                _sfChartArray[indexOfChart].HighLightClosestMarkerOnYAxis(y);
            }
        }

        internal void StopPlay()
        {
            foreach (IChart ic in _sfChartArray)
            {
                ic.RemoveLineSeries();
            }

            _audPlayer.Pause();
            _audPlayer.SeekTo(1);

            _intNumStartZooms = 0;
        }

        internal void PausePlay()
        {

            if (_audPlayer != null)
            {
                _audPlayer.Pause();
            }
        }

        internal void ChangeSpeed(float flSpeedValue)
        {
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in PITCHPLoThELPER: before Change speed, speed is " + flSpeedValue);
#endif          
            bool blWasPlaying = _audPlayer.IsPlaying;
            if (_audPlayer.IsPlaying)
            {
                _audPlayer.Pause(false);
            }
            _audPlayer.ChangeSpeed(flSpeedValue);

            if (blWasPlaying)
            {
                _audPlayer.Play();
            }
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in PITCHPLoThELPER: after Change speed");
#endif


        }

        internal void PlayPlotAndAudio()
        {
            Thread playbackthread = null;

            if (!_audPlayer.IsPlaying)
            {
                playbackthread = new Thread(() =>
               {
                   _audPlayer.Play();
               });

                playbackthread.Start();
            }
        }

        private void SetUpTimer(int intMsPeriod)
        {

            if (_Timer != null)
            {
                _Timer.Enabled = false;
                _Timer.Elapsed -= OnTimedEvent;

                _Timer.Dispose();
                _Timer = null;
            }

            _Timer = new System.Timers.Timer();
            _Timer.Interval = intMsPeriod;

            // Hook up the Elapsed event for the timer. 
            _Timer.Elapsed += OnTimedEvent;

            //m_intListIndex = 0;

            // Have the timer fire repeated events (true is the default)
            _Timer.AutoReset = true;

            _dtTime = DateTime.Now;
            // Start the timer
            _Timer.Enabled = true;
        }


        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            double dblXpt;
            double yVal;
            int i = 0;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            try
            {

                RunOnUiThread(() =>
                {

                    if ((_sfChartArray != null) && (_audPlayer != null))
                    {
                        dblXpt = _audPlayer.CurrentPosition / 1000.0;

                        DrawVerticalLineAtXpoint(dblXpt + Common.Settings.TimeLagSec);

                        if ((!_audPlayer.IsPlaying) && (_intNumStartZooms >= NUM_INIT_ZOOMS))
                        {
                            GraphUpdated(this, new GraphUpdateEventArgs(_audPlayer.CurrentPosition));
                            return;
                        }

                        if (_intNumStartZooms < NUM_INIT_ZOOMS)
                        {
                            _intNumStartZooms++;
                        }

                        i = 0;
                        foreach (IChart ic in _sfChartArray)
                        {
                            yVal = FindClosestYpointToXval(dblXpt + Common.Settings.TimeLagSec, i);

                            if ((_sfChartArray != null) && (_audPlayer != null))
                            {
                                try
                                {
                                    ZoomToPointAndHighLightNote(dblXpt + Common.Settings.TimeLagSec, yVal, i);

                                    GraphUpdated(this, new GraphUpdateEventArgs(_audPlayer.CurrentPosition));
                                }
                                catch (Exception e1)
                                {
                                    string strMsg = e1.Message;
                                    throw new Exception(e1.Message);
                                }
                            }
                            i++;
                        }
                    }

                });
            }
            catch (Exception e1)
            {
                string strMsg = e1.Message;
                throw new Exception(e1.Message);
            }
        }

        private void DrawVerticalLineAtXpoint(double dblXpt)
        {
            if (_sfChartArray != null)
            {
                foreach (IChart ic in _sfChartArray)
                {
                    ic.DrawVerticalLineAtXpoint(dblXpt);
                }
            }

            if (_sfChartOverview != null)
            {
                _sfChartOverview.DrawVerticalLineAtXpoint(dblXpt);
            }
        }

        internal float ViewWindowSecondsGet(float zoomFactor)
        {
            return (float)(zoomFactor * _sfChartArray[0].DataPoints[_sfChartArray[0].DataPoints.Count - 1].XVal);
        }

        internal void GraphClick(object obj, ChartEventArgs.GraphClickedEventArgs args)
        {
            int iTime, rTime;
            int iLeft;
            int iRight;
            float fTime;
            int iStartPos;
            const float ERROR_ADJUST = 0.01f;

            float flCurrentZoomPos = _sfChartArray[0].ZoomPosition_Primary;
            float iCurrentZoomFact = _sfChartArray[0].ZoomFactor_Primary;
            float flWindowWidthSecs;

            flWindowWidthSecs = ViewWindowSecondsGet(_sfChartArray[0].ZoomFactor_Primary);
            // get width of the current graph time window.
            iRight = (args.View).Right;
            iLeft = (args.View).Left;
            fTime = (args.Xpos - iLeft) / (iRight - iLeft);

            // seems to overestime this value so take a bit off

            fTime = fTime - (fTime * ERROR_ADJUST);
            iTime = (int)(fTime * (float)flWindowWidthSecs * 1000);
            iStartPos = (int)((flCurrentZoomPos * _sfChartArray[0].DataPoints[_sfChartArray[0].DataPoints.Count - 1].XVal) * 1000);
            iTime = iStartPos + iTime;

            SeekTo(iTime);
            GraphUpdated(this, new GraphUpdateEventArgs(iTime));
        }

        internal void ReleaseAudio()
        {
            if (_audPlayer != null)
            {
                cEventHelper.RemoveAllEventHandlers(_audPlayer);
                _audPlayer.Stop();
                _audPlayer.Destroy();
                _audPlayer = null;
            }
        }
        internal void CleanUp()
        {
            CommonFunctions comFunc = new CommonFunctions();
            string strMess;

            if (IsDestroyed) { return; }

            try
            {
                if (_Timer != null)
                {
                    _Timer.Elapsed -= OnTimedEvent;
                    _Timer.Enabled = false;
                    _Timer.Dispose();
                    _Timer = null;

                }
                if (this.GraphUpdated != null)
                {
                    cEventHelper.RemoveAllEventHandlers(this.GraphUpdated);
                }

                foreach (IChart ic in _sfChartArray)
                {
                    ic.GraphClick -= GraphClick;
                    ic.ZoomStart -= ZoomStartMethod;
                    cEventHelper.RemoveAllEventHandlers(ic);
                    ic.CleanUp();
                }

                _sfChartOverview.Touch -= GraphOverviewClicked;
                cEventHelper.RemoveAllEventHandlers(_sfChartOverview);
                _sfChartOverview.CleanUp();

                ReleaseAudio();

                IsDestroyed = true;
                Finish();
            }
            catch (Exception e1)
            {
                throw new Exception(e1.Message);
            }
            finally
            {
                //Android.Util.Logger.Info(Common.CommonFunctions.APP_NAME, "Clean up finishing in CleanUp.....");
            }
        }

        internal void SeekTo(int intTimeMs)
        {
            if (_audPlayer != null)
            {
                if (_audPlayer.IsPlaying)
                {
                    _audPlayer.SeekTo(intTimeMs);

                    _audPlayer.Play();
                }
                else
                {
                    _audPlayer.SeekTo(intTimeMs);
                }
            }

            _intNumStartZooms = 0;
        }

        private double FindClosestYpointToXval(double dblXpt, int indexOfChart)
        {
            ISerialisableDataPoint closestPt = null;
            if (_sfChartArray[indexOfChart].DataPoints.Count > 0)
            {
                closestPt = _sfChartArray[indexOfChart].DataPoints.OrderBy(item => Math.Abs(dblXpt - item.XVal)).First();
                return closestPt.YVal;
            }
            return 0;
        }
    }
}
