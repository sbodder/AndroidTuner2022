﻿using Android.App;
using global::Android.Widget;
using Android.OS;
using System;
using AndroidX.AppCompat.App;
using TTtuner_2022_2.Audio;
using TTtuner_2022_2.Plot;
using TTtuner_2022_2.Common;
using TTtuner_2022_2.Fragments;
using TTtuner_2022_2.Adapters;
using Android.Graphics.Drawables;
using System.Collections.ObjectModel;
using TTtuner_2022_2.Services;
using Firebase.Analytics;
using Xamarin.Forms.PlatformConfiguration;
using Android.Views;
using Android;
using Android.Content.PM;
using Android.Content;
using System.Threading.Tasks;
using System.Threading;
using Android.Graphics;
using Android.Media;
using Android.OS.Storage;
using Android.Util;
using System.IO;
using Path = System.IO.Path;
using BE.Tarsos.Dsp.Mfcc;
using AndroidX.Core.Content;
using static AndroidX.Core.Util.Pools;
using System.Runtime.Remoting.Contexts;
using AndroidX.Preference;
using Plugin.CurrentActivity;
using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;

namespace TTtuner_2022_2
{
    [Activity(Label = "TTtuner", Icon = "@drawable/icon", ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private int LED_NOTIFICATION_ID = 0; //arbitrary constant   
        private Audio_Record m_audioRecorder = null;
        private bool blRecordButtonEnabled = true;
        private bool m_blCollectionPaused = true;
        private DataPointHelper<Serializable_DataPoint> m_dataPtHelper;
        private string m_strTimeStampForFileName;
        private bool m_blPermissionsOK = false;
        private int m_secElapsed = 0;
        private int _pitch_i = 0;
        private ImageButton m_RecStopbutton;
        private ImageButton m_PlayPauseButton;
        private AndroidX.ViewPager.Widget.ViewPager m_viewPager;
        private MainFragmentAdapter m_pageAdapter;
        private AnimationDrawable m_animationDrawable;
        GaugeFragment _gaugeFrag;
        ScatterPlotFragment _scatterFrag;
        StatsViewFragment _statsFrag;
        TunerFragment _tunerFrag;
        System.Timers.Timer _buttonTimerClick = null;
        internal Audio_Record AudioRecorder
        {
            get
            {
                return m_audioRecorder;
            }
        }
        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzQwNjc3QDMyMzAyZTMzMmUzMGlONnlkYXFuNlF5TUpOVk40RE1xRmhYWlljeVIxVzZHY0d2eldWdWozZEU9");
                base.OnCreate(bundle);
                CommonFunctions comFunc = new CommonFunctions();

                CrossCurrentActivity.Current.Init(this, bundle);

                //Fabric.Fabric.With(this, new Crashlytics.Crashlytics());
                //Crashlytics.Crashlytics.HandleManagedExceptions();

                //prevent app from sleeping on the main screen
                this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "before request perms");
#endif
                PermissionHelper.RequestPermissions(ref m_blPermissionsOK);

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "ClearAllDataPoints");
#endif

                DataPointCollection.ClearAllDataPoints();
                SetContentView(Resource.Layout.Main);

                // set up string resources

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "before ScalesArrayResource_Get");
#endif
                Common.StringResourceHelper.ScalesArrayResource_Get(this);
                Common.StringResourceHelper.TransposeArrayResource_Get(this);

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "before SetupButtons");
#endif

                SetupButtons();

                var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
                SetSupportActionBar(toolbar);
                SupportActionBar.Title = "TTtuner";



                if (m_blPermissionsOK)
                {
#if Release_LogOutput
                    SetupLogFile();
#endif
                    DoPostPermissionGrantSetup();

                }
            }
            catch (Exception e)
            {
                var str = "Exception : " + e.Message + " Stack trace : " + e.StackTrace;
                Toast.MakeText(this, str, ToastLength.Long).Show();

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, str);
                Logger.FlushBufferToFile();
#endif

                return;
            }
        }
        private void DoPostPermissionGrantSetup()
        {
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting DoPostPermissionGrantSetup");
#endif
            Type type = this.GetType();
            Settings.Init(type, this);
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting SetupFiles");
#endif
            SetupFiles();
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting SetupPageAdapter");
#endif
            SetupPageAdapter();

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting SetupGaugeFragment");
#endif
            SetupGaugeFragment();

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting SetupScatterPlotFragment");
#endif
            SetupScatterPlotFragment();

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting SetCurrentItem");
#endif
            m_viewPager.SetCurrentItem(Common.Settings.PositionOfPageAdapterOnMainActivity, false);
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting StartServiceUsedToHookAppCloseEvent");
#endif
            StartServiceUsedToHookAppCloseEvent();
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting setupAudio");
#endif
            SetupAudio();
            // Give some time for the page adapter to setup (not a synchronous setup) 
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "starting StartRecording");
#endif
            m_audioRecorder.StartRecording(false);

        }
        private bool AllFragementsAreSetup()
        {
            return (_tunerFrag.SetupComplete && _statsFrag.SetupComplete && _scatterFrag.SetupComplete && _gaugeFrag.SetupComplete);
            // return (_tunerFrag.SetupComplete && _statsFrag.SetupComplete );
        }

        private void SetupScatterPlotFragment()
        {
            _scatterFrag = ScatterPlotFragment.NewInstance();
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.chart_frame1, _scatterFrag).Commit();
        }

        private void SetupGaugeFragment()
        {
            _gaugeFrag = GaugeFragment.NewInstance();
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.gauge_frame1, _gaugeFrag).Commit();
        }
        private void SetupButtons()
        {
            m_RecStopbutton = FindViewById<ImageButton>(Resource.Id.RecordButton);
            m_PlayPauseButton = FindViewById<ImageButton>(Resource.Id.PlayPauseButton);
            m_RecStopbutton.Click += OnRecordStopButtonClicked;
            m_PlayPauseButton.Click += OnPlayPauseButtonClicked;

            m_animationDrawable = (AnimationDrawable)AndroidX.Core.Content.ContextCompat.GetDrawable(this, Resource.Drawable.PauseButtonAnimation);
        }
        private void StartPauseButtonAnimation()
        {
            m_PlayPauseButton.SetImageDrawable(m_animationDrawable);
            m_animationDrawable.SetExitFadeDuration(500);
            m_animationDrawable.Start();
        }

        private void SetupPageAdapter()
        {
            m_viewPager = FindViewById<AndroidX.ViewPager.Widget.ViewPager>(Resource.Id.viewpager);
            m_pageAdapter = new MainFragmentAdapter(SupportFragmentManager, this);
            m_viewPager.Adapter = m_pageAdapter;
            m_viewPager.PageSelected += OnPageSelected;
            _statsFrag = (m_pageAdapter.GetItem(1) as StatsViewFragment);
            _tunerFrag = (m_pageAdapter.GetItem(0) as TunerFragment);
        }

        private void SetupLogFile()
        {
            Logger.DeleteLogFile();
            Logger.CreateLogFile();
        }

        private void SetupFiles()
        {
            CommonFunctions cm = new CommonFunctions();
            cm.SetupXmlSettingsAndCsvFiles(this);
        }

        private void SetupAudio()
        {
            m_audioRecorder = new Audio_Record();
            m_audioRecorder.NewPitch += Pitch_received;

            m_dataPtHelper = DataPointCollection.Frq;
        } 
 
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok && requestCode == PermissionHelper.REQUEST_PERMISSIONS_API_30_AND_GREATER)
            {
                PermissionHelper.StorePermissionsResult(data);
            }
            // we have all permissions granted at this point, now do rest of setup
            DoPostPermissionGrantSetup();
        }

        public void OnPageSelected(object sender, AndroidX.ViewPager.Widget.ViewPager.PageSelectedEventArgs e)
        {
#if Trial_Version
                    if (e.Position == 1)
                    {
                        Toast.MakeText(this, "In the Trial version you can only collect Stats for 10 seconds", ToastLength.Long).Show();
                    }
#endif
            Common.Settings.PositionOfPageAdapterOnMainActivity = e.Position;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionHelper.ExitActivityIfAnyPermissionNotGranted(grantResults);
            if (requestCode == PermissionHelper.REQUEST_PERMISSIONS)
            {                
                DoPostPermissionGrantSetup();
            }
            else if (requestCode == PermissionHelper.REQUEST_PERMISSIONS_API_30_AND_GREATER)
            {
                if (!PermissionHelper.AreStoragePermissionsGrantedAndroid11AndGreater())
                {
                    var alert = PermissionHelper.RequestStoragePermissionsAndroid11AndGreater();

                    global::Android.App.Dialog dialog = alert.Create();
                    dialog.Show();
                }
                else
                {
                    DoPostPermissionGrantSetup();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        private void StartServiceUsedToHookAppCloseEvent()
        {
            //Intent downloadIntent = new Intent(this, typeof(TTtunerService));
            //StartService(downloadIntent);
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
#if Release_LogOutput
            MenuInflater.Inflate(Resource.Menu.top_menus_log_output, menu);
#else

            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
#endif

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            if ((m_audioRecorder.IsRecording) && (!blRecordButtonEnabled))
            {
                Toast.MakeText(this, "Please stop recording to access menu items ", ToastLength.Long).Show();
                return base.OnOptionsItemSelected(item);
            }

            if (item.ItemId == Resource.Id.menu_open1)
            {
                var FilePickerAct = new Intent(this, typeof(FilePickerActivity));
                FilePickerAct.PutExtra("MyData", "MainActivity");
                StartActivity(typeof(FilePickerActivity));
                Finish();
            }

            if (item.ItemId == Resource.Id.menu_settings1)
            {
                //open the default file
                StartActivity(typeof(SettingsActivity));
            }

            if (item.ItemId == Resource.Id.menu_trash)
            {
                DeleteStatsGridData();
            }

            if (item.ItemId == Resource.Id.menu_SaveLog)
            {
                Logger.FlushBufferToFile();
            }

            if (item.ItemId == Resource.Id.menu_export_stats)
            {
                m_strTimeStampForFileName = "Stat-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                ContinueCollectionStats(false);
                //save
                SaveStatsFile(m_strTimeStampForFileName);
                ContinueCollectionStats(true);
            }

            return base.OnOptionsItemSelected(item);
        }
        private void ContinueCollectionStats(bool blContinue)
        {
            if (blContinue)
            {
                _statsFrag.UnFreeze();
                StartPauseButtonAnimation();
            }
            else
            {
                m_PlayPauseButton.SetImageResource(Resource.Drawable.PlayButtonMainScreen);
                _statsFrag.Freeze();
            }
            m_blCollectionPaused = !blContinue;
        }
        private void SaveStatsFile(string m_strTimeStampForFileName)
        {
            EditText editTextView = new EditText(this);
            string rtString = m_strTimeStampForFileName;
            AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle("Please Enter File Name");

            editTextView.Text = m_strTimeStampForFileName;

            alert.SetView(editTextView); // <----

            alert.SetPositiveButton("OK", (senderAlert, argus) =>
            {
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    editTextView.Text = editTextView.Text.Replace(c, '_');
                }
                rtString = editTextView.Text;

                m_dataPtHelper.SaveDataPointsToFile(rtString + CommonFunctions.STAT_FILE_EXTENSION);
                Toast.MakeText(this, "File saved. Access via the Archive Menu Item.", ToastLength.Long).Show();

            });

            alert.SetNegativeButton("Cancel", (senderAlert, argus) => { });
            global::Android.App.Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void Pitch_received(object sender, Common.NoteEventArgs e)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            int intSecElap = (int)e.Time;
            //FireBaseEventLogger fb = new FireBaseEventLogger(this);

            if (!AllFragementsAreSetup())
            {
                //fb.SendEvent(fb.events.PITCH_REC_FRAGS_NOT_SETUP, "");
                return;
            }

            if (m_secElapsed != intSecElap)
            {
                m_secElapsed = intSecElap;
                if (!m_blCollectionPaused) { _statsFrag.Refresh(); }
            }


            if (!blRecordButtonEnabled)
            {
                string timeElapsed = comFunc.ConvertSecondsToClockTime(e.Time);
                _tunerFrag.UpdateTimeElapsedText(timeElapsed);

#if Trial_Version
                        if (timeElapsed == "00 : 10")
                        {
                            OnRecordStopButtonClicked(this, new EventArgs());
                        }
#endif
            }

            if (e.Note != "")
            {
                Serializable_DataPoint dp = new Serializable_DataPoint(e.Time, e.Pitch, e.CentsCloseness, e.Note);

                _gaugeFrag.SetGaugePointerValue(e.DbLevel);
                _tunerFrag.UpdateTunerText(e);

                if ((_pitch_i < 100) && (_pitch_i % 10 == 0))
                {
                    //fb.SendEvent(fb.events.PITCH_REC_ADDED_DP, e.Note);
                }
                if (!blRecordButtonEnabled)
                {
                    //we are recording!
                    m_dataPtHelper.AddDataPointToCollection(dp);
                }
                else
                {
                    if (!m_blCollectionPaused) { m_dataPtHelper.AddDataPointToCollection(dp); _statsFrag.AddPointToGrid(dp); }

                    _scatterFrag.AddPointToChart(dp);
                }
            }
            else
            {
                _gaugeFrag.SetGaugePointerValue(-100);
                _scatterFrag.AddPointToChart(new Serializable_DataPoint(e.Time, 0, -60, ""));

                if ((_pitch_i < 50) && (_pitch_i % 10 == 0))
                {
                    //fb.SendEvent(fb.events.PITCH_REC_NO_NOTE, "");
                }

            }
            _tunerFrag.UpdateTunerScales();
            _pitch_i++;

        }

        internal void SaveAudioAndFinishActivity()
        {
            string fileNamePcm = m_strTimeStampForFileName + ".PCM";
            string fileNameWav = m_strTimeStampForFileName + CommonFunctions.WAV_FILE_EXTENSION;

            string strPersonalPath = FileHelper.DataDirectory;
            string strPcmInputFilepath = FileHelper.GetFilePath(fileNamePcm, true);
            string strWavInputFilepath = FileHelper.GetFilePath(fileNameWav, false, MediaStoreHelper.MIMETYPE_WAV);

            if (blRecordButtonEnabled)
            {
                return;
            }

            m_RecStopbutton.SetImageResource(Resource.Drawable.RecordButton);

            blRecordButtonEnabled = !blRecordButtonEnabled;
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In OnStopButtonClick in main activity....");
#endif

            if (m_dataPtHelper.DataPoints.Count <= 10)
            {
                m_dataPtHelper.ClearData();
                AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                dlMessage.SetTitle("No Freqencies detected");
                dlMessage.SetMessage("The App did not detect any note frequencies in the recording - Need to detect note frequencies in order to save and graph a recording");
                dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlMessage.Show();
                return;
            }

            Toast.MakeText(this, "File saved. Access via the Archive Menu Item. Loading File.... ", ToastLength.Long).Show();

            Task.Run(() =>
            {
                m_audioRecorder.StopRecording();

                m_dataPtHelper.SaveDataPointsToFile(m_strTimeStampForFileName + CommonFunctions.TEXT_EXTENSION);
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In OnStopButtonClick in main activity: txt file name is :" + m_strTimeStampForFileName + CommonFunctions.TEXT_EXTENSION);
#endif
                m_dataPtHelper.ClearData();


                // make sure that the m_audioRecorder has completed writing the audio data to the file
                while (m_audioRecorder.AudioRecorder != null)
                {
                    Thread.Sleep(100);
                }

                ConvertPcmToWave wv = new ConvertPcmToWave(m_audioRecorder.SampleRate, 1, strPcmInputFilepath, strWavInputFilepath);
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In OnStopButtonClick in main activity: wave file name is :" + strWavInputFilepath);
#endif
                string[] stringArr = { strWavInputFilepath , Common.Settings.DisplayNotesGraph.ToString(),
                                            Common.Settings.DisplayDecibelGraph.ToString() ,
                                             Common.Settings.GraphOverlay   };
                var SecondActivity = new Intent(this, typeof(GraphScreenActivity));
                SecondActivity.PutExtra("MyData", stringArr);
                StartActivity(SecondActivity);
            });

            Finish();
        }
        internal void DeleteStatsGridData()
        {
            if (!blRecordButtonEnabled)
            {
                return;
            }

            _statsFrag.DeleteAllData();
            DataPointCollection.ClearAllDataPoints();
        }
        private void OnPlayPauseButtonClicked(object sender, EventArgs e)
        {
            if (!blRecordButtonEnabled)
            {
                return;
            }

            if (m_blCollectionPaused)
            {
                Toast.MakeText(this, "Collecting Stats...", ToastLength.Short).Show();
                ContinueCollectionStats(true);
#if Trial_Version
                        if (_buttonTimerClick == null)
                        {
                            _buttonTimerClick = new System.Timers.Timer();
                            _buttonTimerClick.Interval = 10000;
                            _buttonTimerClick.Enabled = true;
                            _buttonTimerClick.Elapsed += (object s, System.Timers.ElapsedEventArgs evt) =>
                            {
                                RunOnUiThread(() =>
                                {
                                    _statsFrag.DeleteAllData();
                                    Toast.MakeText(this, "Trial Version - Stats deleted after 10 seconds", ToastLength.Short).Show();
                                });
                            };
                        }
                        else
                        {
                            _buttonTimerClick.Enabled = true;
                        }
#endif
            }
            else
            {
#if Trial_Version
                        if (_buttonTimerClick != null) { _buttonTimerClick.Stop(); }
#endif
                ContinueCollectionStats(false);
            }
        }
        internal void OnRecordStopButtonClicked(object sender, EventArgs e)
        {
            if (blRecordButtonEnabled)
            {
                DataPointCollection.ClearAllDataPoints();
                m_audioRecorder.NewPitch -= Pitch_received;

                m_RecStopbutton.SetImageResource(Resource.Drawable.StopButton1);
                m_PlayPauseButton.SetImageResource(Resource.Drawable.PauseDisabledButtonMainScreen);
                m_audioRecorder.StopRecording();

                // make sure that the m_audioRecorder has completed its destruction before continuing
                while (m_audioRecorder.AudioRecorder != null)
                {
                    Thread.Sleep(100);
                }

                m_strTimeStampForFileName = "Rec-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

                m_audioRecorder.NewPitch += Pitch_received;

                m_audioRecorder.StartRecording(true, m_strTimeStampForFileName + ".PCM");

                blRecordButtonEnabled = !blRecordButtonEnabled;

#if Trial_Version
                        if (_buttonTimerClick != null) { _buttonTimerClick.Stop(); }
                        Toast.MakeText(this, "Recording will end after 10 seconds as this is a trial version of TTtuner...", ToastLength.Long).Show();
#else
                Toast.MakeText(this, "Recording Audio...", ToastLength.Short).Show();
#endif

            }
            else
            {
                SaveAudioAndFinishActivity();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_buttonTimerClick != null)
            {
                _buttonTimerClick.Stop();
                _buttonTimerClick = null;
            }
            m_pageAdapter.Dispose();
            m_audioRecorder.Destroy();
            m_RecStopbutton.Click -= OnRecordStopButtonClicked;
            m_viewPager.PageSelected -= OnPageSelected;
            Finish();
        }
        protected override void OnPause()
        {
            base.OnPause();
            if (m_audioRecorder != null)
            {
                if ((m_audioRecorder.IsRecording) && (blRecordButtonEnabled))
                {
                    m_audioRecorder.StopRecording();
                }
                else if ((m_audioRecorder.IsRecording) && (!blRecordButtonEnabled))
                {
                    // flash leds
                    BlueFlashLight();
                }
            }
        }
        private void BlueFlashLight()
        {
            NotificationManager nm = (NotificationManager)GetSystemService(NotificationService);
            Notification notif = new Notification();
            notif.Defaults = Notification.ColorDefault;
            notif.LedARGB = Color.Blue;
            notif.Flags = NotificationFlags.ShowLights | NotificationFlags.AutoCancel | NotificationFlags.NoClear;
            notif.LedOnMS = 100;
            notif.LedOffMS = 100;
            notif.Icon = Resource.Drawable.Icon;
            Bitmap b1;
            b1 = BitmapFactory.DecodeResource(Resources, Resource.Drawable.Icon);
            notif.LargeIcon = b1;

            notif.ContentView = new RemoteViews(PackageName, Resource.Layout.NotificationLayout);
            nm.Notify(LED_NOTIFICATION_ID, notif);
        }

        protected override void OnResume()
        {
            base.OnResume();
            string strMess;
            // m_prgBAr.Visibility = ViewStates.Gone;

            if (m_blPermissionsOK)
            {
                try
                {
                    if (m_audioRecorder != null)
                    {
                        if ((!m_audioRecorder.IsRecording) && (blRecordButtonEnabled))
                        {
                            m_audioRecorder.StartRecording(false);
                        }
                        else
                        {
                            // stop flashing leds
                            CancelBlueFlashLight();
                        }
                    }
                }
                catch (Exception e1)
                {
                    strMess = e1.Message;
                    throw new Exception(e1.Message);
                }
            }
        }  
        private void CancelBlueFlashLight()
        {
            NotificationManager nm = (NotificationManager)GetSystemService(NotificationService);
            nm.Cancel(LED_NOTIFICATION_ID);
            nm.CancelAll();
        }
    }
}

