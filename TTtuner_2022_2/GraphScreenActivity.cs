using Android.App;
using global::Android.Widget;
using Android.OS;
using Android.Media;
using Com.Syncfusion.Charts;

using System.Diagnostics;


using System;
using Android.Util;
using System.Collections.Generic;
using TTtuner_2022_2;
using System.IO;
using global::Android.Content;
using Android.Graphics;
using System.Collections.ObjectModel;
using TTtuner_2022_2.Plot;
using global::Android.Views;
using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using System.Reflection;
using Android.Graphics.Drawables;
using TTtuner_2022_2.PopUpDialogFragments;
using TTtuner_2022_2.Common;
using BE.Tarsos.Dsp.IO.Android;
using System.Threading;

namespace TTtuner_2022_2.Plot
{



    [Activity(Label = "Graph", ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize)]
    internal class GraphScreenActivity : AppCompatActivity
    {
        int m_intElapsedTime = 0;
        Button _rewind;
        Button _fastForward;
        // SeekBar _seekBar;
        LinearLayout m_llay;
        LinearLayout m_llayOverview;
        Button _stats;
        Button _changeSpeed;
        TextView _timeElapsed;
        private bool _displayDecibelGraph;
        private bool _displayNotesGraph;
        string _graphOverlay;
        private ProgressBar m_prgBAr;

        bool m_blIsTRackingTouchOnSeekBar = false;

        Button _stopPlay, _play;
        // PlotView plotView;
        Plot.PlotHelper m_pchPlotHelper;
        string[] m_strSettings;
        string m_strWavFilePath;
        string m_strTextFilePath;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            float m_flPlayerSpeed = 1f;      
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            
            base.OnCreate(savedInstanceState);

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            m_strSettings = Intent.GetStringArrayExtra("MyData");
            m_strWavFilePath = m_strSettings[0];
            _displayNotesGraph = m_strSettings[1] == "True" ? true: false;
            _displayDecibelGraph = m_strSettings[2] == "True" ? true : false;
            _graphOverlay = m_strSettings[3];
            // Create your application here

            SetContentView(Resource.Layout.GraphScreen);

            
            //_startRec = FindViewById<Button>(Resource.Id.startRec);
            //    _stopRec = FindViewById<Button>(Resource.Id.stopRec);
            _play = FindViewById<Button>(Resource.Id.play);
            //_stopPlay = FindViewById<Button>(Resource.Id.stopPlay);
            // _pause = FindViewById<Button>(Resource.Id.pausePlay);
            _changeSpeed = FindViewById<Button>(Resource.Id.ChangeSpeed);

            _stopPlay = FindViewById<Button>(Resource.Id.stopPlay);

            _rewind = FindViewById<Button>(Resource.Id.Rewind);

            _fastForward = FindViewById<Button>(Resource.Id.FastForward);

            _stats = FindViewById<Button>(Resource.Id.StatsButton);

            m_prgBAr = FindViewById<ProgressBar>(Resource.Id.PrgBar2);

            m_prgBAr.Visibility = ViewStates.Gone;



            try
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In graph on create");
#endif
                //this sets up the taros dsp ffmeg binaries
                AndroidFFMPEGLocator affmeg = new AndroidFFMPEGLocator(this);

                DataPointCollection.ClearAllDataPoints();
                SetupGraphViews();
            }
            catch (Exception e1)
            {
                Toast.MakeText(this, "EXception in graph on create :" + e1.Message, ToastLength.Long).Show();
                throw e1;
            }

            _rewind.Click += delegate
            {

                string strMEss;
                // go back by one second
                try
                {
                    m_pchPlotHelper.SeekTo(m_intElapsedTime - 1000 < 0 ? 0 : m_intElapsedTime - 1000);
                }
                catch (Exception e1)
                {
                    throw new Exception(e1.Message);
                }

            };
            
            _fastForward.Click += delegate
            {

                string strMEss;
                // go fwd by one second

                try
                {
                    m_pchPlotHelper.SeekTo(m_intElapsedTime + 1000);
                }
                catch (Exception e1)
                {
                    throw new Exception(e1.Message);
                }

            };

            _timeElapsed = FindViewById<TextView>(Resource.Id.txtTimeElapsed);

            _stats.Click += StatsClick;            

            _play.Click += PlayClicked;

            _stopPlay.Click += delegate
            {

                m_pchPlotHelper.StopPlay();
                _play.SetBackgroundResource(Resource.Drawable.PlayButton);

            };


            _changeSpeed.Click += delegate
            {

                switch (m_flPlayerSpeed)
                {
                    case 1f:
                        m_flPlayerSpeed = 0.5f;
                        break;

                    case 0.5f:
                        //  m_flPlayerSpeed = 0.25f;
                        m_flPlayerSpeed = 0.25f;
                        break;

                    case 0.25f:
                        m_flPlayerSpeed = 1f;
                        break;

                    default:
                        break;
                }

                // if ( m_flPlayerSpeed ==
                _changeSpeed.Text = String.Format("{0:0.##}", m_flPlayerSpeed) + "x";
                m_pchPlotHelper.ChangeSpeed(m_flPlayerSpeed);

            };


            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar2);
            SetSupportActionBar(toolbar);
            string strFilename = m_strWavFilePath.Substring(m_strWavFilePath.LastIndexOf('/') + 1, m_strWavFilePath.LastIndexOf('.') - 1 - m_strWavFilePath.LastIndexOf('/'));

            SupportActionBar.Title = "TTtuner   " + comFunc.TruncateStringLeft(strFilename, 15);

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In graph on create: exit function");
#endif
        }

        internal void StatsClick(object sender, EventArgs args)
        {
            m_prgBAr.Visibility = ViewStates.Visible;
            _stats.Visibility = ViewStates.Gone;
            //dialog.Show(FragmentManager, "dialog");

            if (m_pchPlotHelper.IsPlaying)
            {
                m_pchPlotHelper.PausePlay();
                // change button  image to play
                _play.SetBackgroundResource(Resource.Drawable.PlayButton);
            }
            this.RunOnUiThread(() =>
            {
                try
                {
                    GC.Collect();
                    Logger.Info(Common.CommonFunctions.APP_NAME, "stats button click -> starting stats act....");
                    string[] strArray = { m_strTextFilePath, _displayNotesGraph.ToString(), _displayDecibelGraph.ToString(), true.ToString(), false.ToString() };
                    Intent _intent = new Intent(this, new StatsActivity().Class);
                    _intent.PutExtra("MyData", strArray);
                    
                    StartActivity(_intent);
                }
                catch (Exception e1)
                {
                    Logger.Info(Common.CommonFunctions.APP_NAME, "Exception in graphscreen act!!....");
                    throw e1;
                }
            });
        }

        private void SetupGraphViews()
        {
            int intDurationOfWaveFile;
            intDurationOfWaveFile = SetupPitchPlotHelper();
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In graph on create: finished SetupPitchPlotHelper");
#endif


#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In graph on create: finished StopTrackingTouch add");
#endif

            m_llay = (LinearLayout)FindViewById(Resource.Id.LinearLayoutForGraph);

            for (int i = 0; i < m_pchPlotHelper.SF_ArrayOfCharts.Length; i++)
            {
                LinearLayout m_llayTemp = new LinearLayout(this);
                LinearLayout.LayoutParams parameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 0, 1);
                m_llayTemp.LayoutParameters = parameters;
                m_llayTemp.Orientation = global::Android.Widget.Orientation.Vertical;
                m_llayTemp.AddView(m_pchPlotHelper.SF_ArrayOfCharts[i] as View);
                m_llay.AddView(m_llayTemp);
            }

            m_llayOverview = (LinearLayout)FindViewById(Resource.Id.LinearLayoutForGraphOverview);
            m_llayOverview.AddView(m_pchPlotHelper.SF_ChartOverview);
        }

        private int SetupPitchPlotHelper()
        {
            string strTimeStamp;

            int intDurationOfWaveFile;

            Factory fct = new Factory();
            strTimeStamp = m_strWavFilePath.Substring(0, m_strWavFilePath.Length - 4);
            m_strTextFilePath = strTimeStamp + ".TXT";

            m_pchPlotHelper = fct.CreatePlotHelper(this, m_strTextFilePath, m_strWavFilePath, _displayNotesGraph, _displayDecibelGraph, _graphOverlay);

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In SetupPitchPlotHelper in graph activity: wave file paths is :" + m_strWavFilePath );
#endif
            intDurationOfWaveFile = m_pchPlotHelper.Duration;

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In SetupPitchPlotHelper in graph activity: intDurationOfWaveFile is :" + intDurationOfWaveFile);
#endif
            // m_pchPlotHelper.LoadDataPointsFromFile(m_strTextFilePath);

            m_pchPlotHelper.GraphUpdated += GraphUpdatedOnView;

            // m_pchPlotHelper.GraphClicked += PlayClicked;
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In SetupPitchPlotHelper in graph activity: exiting.... " + intDurationOfWaveFile);
#endif

            return intDurationOfWaveFile;
        }


        public override void OnBackPressed()
        {

            m_pchPlotHelper.StopPlay();
            Finish();
            // always go back to main activity

            Task.Run(() =>
            {
                StartActivity(typeof(MainActivity));
            });


        }

        internal void PlayClicked(object sender, EventArgs e)
        {
            string strMEss;

            try
            {
                if (m_pchPlotHelper.IsPlaying)
                {
                    m_pchPlotHelper.PausePlay();
                    // change button  image to play
                    _play.SetBackgroundResource(Resource.Drawable.PlayButton);
                }
                else
                {
#if Release_LogOutput
                    Logger.Info(Common.CommonFunctions.APP_NAME, "Play pressed.... " );
#endif
                    m_pchPlotHelper.PlayPlotAndAudio();
                    // change button  image to pause

                    _play.SetBackgroundResource(Resource.Drawable.PauseButton);

                }
            }
            catch (Exception e1)
            {
                throw new Exception(e1.Message);
            }
        }



        private void GraphUpdatedOnView(object sender, PlotHelper.GraphUpdateEventArgs e)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();

            if (!m_blIsTRackingTouchOnSeekBar)
            {

                m_intElapsedTime = e.CurrrentPositionMS;

                _timeElapsed.Text = comFunc.ConvertSecondsToClockTime(e.CurrrentPositionMS / 1000);

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "GraphUpdatedOnView : playing audio and current position is : " + e.CurrrentPositionMS );
#endif


            }
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
#if Release_LogOutput
            MenuInflater.Inflate(Resource.Menu.top_menus_log_output, menu);
#else

            MenuInflater.Inflate(Resource.Menu.Graph, menu);
#endif
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_open1)
            {
                StartActivity(typeof(FilePickerActivity));
                m_pchPlotHelper.StopPlay();
                //kill this activity
                Finish();
            }

            if (item.ItemId == Resource.Id.menu_settings1)
            {
                StartActivity(typeof(SettingsActivity));
            }

            if (item.ItemId == Resource.Id.menu_record1)
            {

                StartActivity(typeof(MainActivity));

                m_pchPlotHelper.StopPlay();
                //kill this activity
                Finish();
            }

            if (item.ItemId == Resource.Id.menu_SaveLog)
            {
                Logger.FlushBufferToFile();
            }

            if (item.ItemId == Resource.Id.menu_configure1)
            {
                bool displayNotes = _displayNotesGraph, displayDecibel= _displayDecibelGraph;
                string graphOverlay = _graphOverlay;
                var dialog = PopUpGraphConfiguration.NewInstance(
                this.Resources.GetString(Resource.String.GraphConfig_prompt),
                                        _graphOverlay,
                                        (bool)_displayNotesGraph,
                                        (bool)_displayDecibelGraph,
                                        this);
                dialog.OkPressed += (s, strArr) =>
                {
                    _displayNotesGraph = strArr[0] == "True" ? true : false;
                    _displayDecibelGraph = strArr[1] == "True" ? true : false;
                    _graphOverlay = strArr[2];

                    if ((graphOverlay != _graphOverlay) || (displayNotes != _displayNotesGraph) || (displayDecibel != _displayDecibelGraph))
                    {
                        //settings have changes reload the page

                        m_pchPlotHelper.StopPlay();
                        m_pchPlotHelper.ReleaseAudio();
                        Finish();

                        string[] settingsArray = {m_strWavFilePath, _displayNotesGraph.ToString(), _displayDecibelGraph.ToString(),
                                                    _graphOverlay };

                        var SecondActivity = new Intent(this, typeof(GraphScreenActivity));
                        
                        SecondActivity.PutExtra("MyData", settingsArray);

                        StartActivity(SecondActivity);                        
                    }
                };

                dialog.Show(this.FragmentManager, "dialog");
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnPause()
        {
            Logger.Info(Common.CommonFunctions.APP_NAME, "in OnPause in graph screen act....");
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ondestroy in the graphs screen... ");

            try
            {
                m_pchPlotHelper.CleanUp();
                m_llay.RemoveAllViews();
            }
            catch (Exception e1)
            {
                string strMes;
                strMes = e1.Message;
#if Release_LogOutput || DEBUG
                Logger.Info(Common.CommonFunctions.APP_NAME, "In OnDestroy in Graph activity, got an exception : " + e1.Message);
#endif
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

#if Release_LogOutput || DEBUG
            Logger.Info(Common.CommonFunctions.APP_NAME, "In OnResume in Graph activity");
#endif
            m_pchPlotHelper.ReLoadSettings();
            m_prgBAr.Visibility = ViewStates.Gone;
            _stats.Visibility = ViewStates.Visible;
        }
    }
}