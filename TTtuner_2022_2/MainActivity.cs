using Android.App;
using global::Android.Widget;
using Android.OS;



using System;
//using Android.Util;
//using System.Collections.Generic;
//using System.IO;
//using global::Android.Views;
//using Android.Graphics;
//using System.Threading.Tasks;
//using System.Threading;
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

namespace TTtuner_2022_2
{
    [Activity(Label = "TTtuner",  Icon = "@drawable/icon", ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity
    {
        private int LED_NOTIFICATION_ID = 0; //arbitrary constant   
        private int REQUEST_PERMISSIONS = 3; //arbitrary constant
        private Audio_Record m_audioRecorder = null;
        private LinearLayout m_llay;
        private bool ignoreSwipeleftToRightGestures = false;

        private TextView m_txtTimeElapsed;

        private bool blRecordButtonEnabled = true;
        private bool m_blCollectionPaused = true;
        private DataPointHelper<Serializable_DataPoint> m_dataPtHelper;
        private string m_strTimeStampForFileName;
        private bool m_blPermissionsOK = false;
        private int m_secElapsed = 0;
        private int _pitch_i = 0;


        internal Audio_Record AudioRecorder
        {
            get
            {
                return m_audioRecorder;
            }
        }

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


        protected override void OnCreate(Bundle bundle)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzMyMzk5QDMxMzgyZTMzMmUzMEdZWU1sallKYjF3eU1tRi9HQUpCOUlRTmlXM0xNTE1TUHZ5aHRCZnlGc289");
            base.OnCreate(bundle);
            CommonFunctions comFunc = new CommonFunctions();

            //Fabric.Fabric.With(this, new Crashlytics.Crashlytics());
            //Crashlytics.Crashlytics.HandleManagedExceptions();

            //prevent app from sleeping on the main screen
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            RequestPermissions();
            DataPointCollection.ClearAllDataPoints();
            SetContentView(Resource.Layout.Main);

            // set up string resources

            Common.StringResourceHelper.ScalesArrayResource_Get(this);
            Common.StringResourceHelper.TransposeArrayResource_Get(this);

            SetupButtons();

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "TTtuner";

            if (m_blPermissionsOK)
            {
                DoPostPermissionGrantSetup();
#if Release_LogOutput
                SetupLogFile();
#endif
            }
        }

        private void DoPostPermissionGrantSetup()
        {
            Type type = this.GetType();
            Settings.Init(type, this);
            SetupFiles();
            SetupPageAdapter();
            SetupGaugeFragment();
            SetupScatterPlotFragment();
            m_viewPager.SetCurrentItem(Common.Settings.PositionOfPageAdapterOnMainActivity, false);
            StartServiceUsedToHookAppCloseEvent();
            SetupAudio();
            // Give some time for the page adapter to setup (not a synchronous setup) 
            m_audioRecorder.StartRecording(false);
        }

        private bool AllFragementsAreSetup()
        {
            return (_tunerFrag.SetupComplete && _statsFrag.SetupComplete && _scatterFrag.SetupComplete && _gaugeFrag.SetupComplete);
        }

        private void SetupScatterPlotFragment()
        {
            _scatterFrag = ScatterPlotFragment.NewInstance(this);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.chart_frame1, _scatterFrag).Commit();
        }

        private void SetupGaugeFragment()
        {
            _gaugeFrag = GaugeFragment.NewInstance(this);
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
            cm.SetupXmlSettingsFile(this);
        }

        private void SetupAudio()
        {
            m_audioRecorder = new Audio_Record();
            m_audioRecorder.NewPitch += Pitch_received;

            m_dataPtHelper = DataPointCollection.Frq;
        }


        private void RequestPermissions()
        {

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) == (int)Permission.Granted
          && AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) == (int)Permission.Granted)

                {
                    m_blPermissionsOK = true;
                }
                else
                {
                    AndroidX.Core.App.ActivityCompat.RequestPermissions(this,
                        new String[] { Manifest.Permission.RecordAudio, Manifest.Permission.Internet },
                        REQUEST_PERMISSIONS);
                }
            }
            else
            {
                if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted
               && AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted
              )
                {
                    // We have permissions, go ahead and use the app
                    m_blPermissionsOK = true;
                }
                else
                {
                    m_blPermissionsOK = false;
                    //  permissions are not granted. If necessary display rationale & request.
                    AndroidX.Core.App.ActivityCompat.RequestPermissions(this,
                           new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.RecordAudio, Manifest.Permission.Internet },
                           REQUEST_PERMISSIONS);
                }
            }
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
            if (requestCode == REQUEST_PERMISSIONS)
            {
                foreach (Permission p1 in grantResults)
                {
                    if (!(p1 == Permission.Granted))
                    {
                        Toast.MakeText(this, "Application Exiting...", ToastLength.Long).Show();
                        FinishAffinity();
                    }
                }
                DoPostPermissionGrantSetup();
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

                m_dataPtHelper.SaveDataPointsToFile(rtString + ".STT");
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
            string fileNameWav = m_strTimeStampForFileName + ".WAV";

            string strPersonalPath = FileHelper.DataDirectory;
            string strPcmInputFilepath = System.IO.Path.Combine(strPersonalPath, fileNamePcm);
            string strWavInputFilepath = System.IO.Path.Combine(strPersonalPath, fileNameWav);

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

                m_dataPtHelper.SaveDataPointsToFile(m_strTimeStampForFileName + ".TXT");
#if Release_LogOutput
                        Logger.Info(Common.CommonFunctions.APP_NAME, "In OnStopButtonClick in main activity: txt file name is :" + m_strTimeStampForFileName + ".TXT");
#endif
                m_dataPtHelper.ClearData();


                // make sure that the m_audioRecorder has completed writing the audio data to the file
                while (m_audioRecorder.AudioRecorder != null)
                {
                    Thread.Sleep(100);
                }

                ConvertPcmToWave wv = new ConvertPcmToWave(m_audioRecorder.SampleRate, 1, strPcmInputFilepath, strWavInputFilepath);
#if Release_LogOutput
                        Logger.Info(Common.CommonFunctions.APP_NAME, "In OnStopButtonClick in main activity: wave file name is :" + strWavInputFilepath );
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

