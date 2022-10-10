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
using TTtuner_2022_2.PopUpDialogFragments;
using TTtuner_2022_2.Music;
using System.Reflection;
using Android.Content.PM;
using System.IO;
using Android.Database;
using Android.Provider;
using Android.Util;
using Com.Github.Angads25.Filepicker.Controller;
using Com.Github.Angads25.Filepicker.Model;
using Com.Github.Angads25.Filepicker.View;
using AndroidX.AppCompat.App;
using TTtuner_2022_2.EventHandlersTidy;

namespace TTtuner_2022_2
{

    [Activity(Label = "Settings", ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize)]
    internal class SettingsActivity : AppCompatActivity
    {
        TextView textTuningSystem;

        TextView textA4ref;
        TextView textTimeLAg;
        TextView textDataFolder;
        TextView textPitchSamplingFreq;
        TextView textRestoreDefaults;
        TextView textCredits;
        TextView textGraphConfig;
        TextView textMinNumberOfSamplesForNote;
        TextView textNoteDetectionClarity;
        private FilePickerDialog m_FileDialog;

        TextView textVersionNumber;
        
        Spinner m_spinnerTranspose;
        Spinner spinnerSampleRate;
        Spinner spinnerAudioPlayer;

        internal const int READ_EXTERNAL_FOLDER = 1;
        internal int TimeLagMs { get; set; }

        internal int PitchSamplingFrequency { get; set; }
        FileSelectionListener lis1;

        internal class FileSelectionListener : Java.Lang.Object, IDialogSelectionListener, IJavaObject, IDisposable
        {
            internal Activity act;

            internal event EventHandler<string> FolderSelected;

            //public void 

            public void OnSelectedFilePaths(string[] p0)
            {


                int i = p0.Length;

                Common.Settings.DataFolder = p0[0];

                FolderSelected(this, p0[0]); 
            }
        }

        private void FolderUpdate(object sender, string e)
        {
            textDataFolder.Text = e;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);             

            string[] m_arrStringPitchFreq = { "100", "50" , "25", "20" };
            Common.CommonFunctions comFunc = new Common.CommonFunctions();

            SetContentView(Resource.Layout.SettingsScreen);
            textTuningSystem = FindViewById<TextView>(Resource.Id.textTuningSystem);
            textVersionNumber = FindViewById<TextView>(Resource.Id.textVersionNumber);
            textA4ref = FindViewById<TextView>(Resource.Id.textA4Ref);
            textDataFolder = FindViewById<TextView>(Resource.Id.textDataFolder);
            textRestoreDefaults = FindViewById<TextView>(Resource.Id.textRestoreDefaults);
            textGraphConfig = FindViewById<TextView>(Resource.Id.textGraphConfiguration);

            textCredits = FindViewById<TextView>(Resource.Id.textCredits);            

            textCredits.Click += (sender, args) =>
            {
                StartActivity(typeof(CreditsActivity));

            };

            textGraphConfig.Click += GraphConfigEventHandler;
            textRestoreDefaults.Click += (sender, args) =>
             {
                 AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                 dlMessage.SetTitle("Please Confirm");
                 dlMessage.SetMessage("Are you sure you wish to restore the default settings?");
                 dlMessage.SetNegativeButton("Cancel", (senderAlert, argus) => { });
                 dlMessage.SetPositiveButton("OK", (senderAlert, argus) =>
                 {

                     Common.Settings.ResetAndReloadSettings(this);

                     LoadSettingsDisplayValues();
                 });

                 dlMessage.Show();
             };

            textDataFolder.Click += (sender, args) =>
            {

                lis1 = new FileSelectionListener();
                //  string[] strExt = { "wav" };
                Java.IO.File ExternalDir = new Java.IO.File(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);

                lis1.act = this;

                lis1.FolderSelected += FolderUpdate;

                try
                {
                    DialogProperties properties = new DialogProperties();
                    m_FileDialog = new FilePickerDialog(this, properties);

                    m_FileDialog.SetTitle("Select Folder");
                    m_FileDialog.SetPositiveBtnName("Select");
                    m_FileDialog.SetNegativeBtnName("Cancel");

                    properties.SelectionMode = DialogConfigs.MultiMode;

                    properties.SelectionType = DialogConfigs.DirSelect;


                    properties.Root = ExternalDir;

                    m_FileDialog.Properties = properties;

                    m_FileDialog.SetDialogSelectionListener(lis1);
                    m_FileDialog.Show();
                }
                catch (Exception ex)
                {
                    AndroidX.AppCompat.App.AlertDialog.Builder dlErrorMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                    dlErrorMessage.SetTitle("Error");
                    dlErrorMessage.SetMessage("Could not imports files. EXception :" + ex.Message);
                    dlErrorMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                    dlErrorMessage.Show();
                }



            };

            textTuningSystem.Click += (sender, args) =>
            {
                var dialog = PopUpForTuningSystemSelectionDialog.NewInstance(Common.Settings.TuningSytem, Common.Settings.RootScale);


                dialog.ValueSet += (sender2, value) =>
                {
#if !Trial_Version
                    textTuningSystem.Text = value[0] == "Equal Temperament" ? value[0] : value[0] + " ( " + value[1] + " )";

                    Common.Settings.TuningSytem = value[0];

                    Common.Settings.RootScale = value[1];
#else
                    AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                    dlMessage.SetTitle("Feature not enabled");
                    dlMessage.SetMessage("This feature is not enabled in the trial version of TTtuner. The full version can be purchased in the Google Play Store.");
                    dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });

              
                    dlMessage.Show();
#endif
                };


                dialog.Show(FragmentManager, "dialog");
            };

            textA4ref.Click += (sender, args) =>
            {
#if !Trial_Version
                var dialog = PopUpWithSeekerDialog.NewInstance(390, 500, 0.1, Common.Settings.A4refDouble);
                dialog.Title = "A4 Reference Adjustment (Hz)";

                dialog.Description = "";

                dialog.ValueSet += (sender2, value) =>
                {
                    Common.Settings.A4ref = value;
                    textA4ref.Text = value + " Hz";
                };

                dialog.Show(SupportFragmentManager, "dialog");
#else
                AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                dlMessage.SetTitle("Feature not enabled");
                dlMessage.SetMessage("This feature is not enabled in the trial version of TTtuner. The full version can be purchased in the Google Play Store.");
                dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlMessage.Show();
#endif
            };            

            textPitchSamplingFreq = FindViewById<TextView>(Resource.Id.textPitchSamplingFreq);

            textPitchSamplingFreq.Click += (sender, args) =>
            {
                AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                alert.SetTitle("Pitch Sampling Frequency (Hz)");

                alert.SetItems(m_arrStringPitchFreq, (sender2, args2) =>
                {
                    Common.Settings.PitchSmaplingFrequency = m_arrStringPitchFreq[args2.Which];
                    //  comFunc.WriteXmlSetting("PitchSmaplingFrequency", m_arrStringPitchFreq[args2.Which]);
                    textPitchSamplingFreq.Text = m_arrStringPitchFreq[args2.Which] + " Hz";
                });

                global::Android.App.Dialog dialog = alert.Create();
                dialog.Show();
            };

            textTimeLAg = FindViewById<TextView>(Resource.Id.textTimeLagValue);        

            textTimeLAg.Click += (sender, args) =>
            {
                var dialog = PopUpWithSeekerDialog.NewInstance(0, 1000, 1, Convert.ToInt32(Common.Settings.TimeLagString));
                dialog.Title = "Time Lag Adjustment (ms)";

                dialog.Description = "The Time Lag should be adjusted when the pitch graph and the recorded audio are not synching correctly. \n\nBefore adjusting Time Lag it is suggested that you first adjust the 'Pitch Sampling'";
                dialog.Description += " and 'Recording Sampling Rate' settings to their lowest value, re-record a track and see if there is still a synch issue with playback.";

                dialog.ValueSet += (sender2, value) =>
                {
                    Common.Settings.TimeLagString = value;
                    textTimeLAg.Text = value;
                };

                dialog.Show(SupportFragmentManager, "dialog");
            };

            textMinNumberOfSamplesForNote = FindViewById<TextView>(Resource.Id.textMinNumberOfSamplesForNote);

            textMinNumberOfSamplesForNote.Text = Common.Settings.MinNumberOfSamplesForNote.ToString();
            textMinNumberOfSamplesForNote.Click += (sender, args) =>
            {
                var dialog = PopUpWithSeekerDialog.NewInstance(1, 100, 1, Common.Settings.MinNumberOfSamplesForNote);
                dialog.Title = "Minimum Number of Samples per Note";
                dialog.Description = "This setting specifies the minimum of number of samples required for a particular note to be displayed on the Stats table";
                

                dialog.ValueSet += (sender2, value) =>
                {
                    Common.Settings.MinNumberOfSamplesForNote = Convert.ToInt32(value);
                    textMinNumberOfSamplesForNote.Text = value;
                };

                dialog.Show(SupportFragmentManager, "dialog");
            };

            textNoteDetectionClarity = FindViewById<TextView>(Resource.Id.textNoteDetectionClarity);
            textNoteDetectionClarity.Text = Common.Settings.NoteClarity;
            textNoteDetectionClarity.Click += OnTextNoteDetectionClarityClicked;



            m_spinnerTranspose = FindViewById<Spinner>(Resource.Id.spinnerTranspose);
             var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.TransposeSelection_array, Resource.Layout.spinner_item);


            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            m_spinnerTranspose.Adapter = adapter;

            m_spinnerTranspose.ItemSelected += (sender, args) =>
            {
#if !Trial_Version
                string strTranspose;
                int intTranspositionOffset;
                string[] strArrTranspose = Common.StringResourceHelper.TransposeArrayResource_Get(this);

                Common.Settings.Transpose = m_spinnerTranspose.Adapter.GetItem(args.Position).ToString();

                strTranspose = m_spinnerTranspose.Adapter.GetItem(args.Position).ToString();

                intTranspositionOffset = comFunc.GetIndexOfItemInstringArray(strArrTranspose, strTranspose);
                NotePitchMap.SetTranpositionOffset(intTranspositionOffset);


#else
                string strTranspose;
                int intTranspositionOffset;
                string[] strArrTranspose = Common.StringResourceHelper.TransposeArrayResource_Get(this);


                if (m_spinnerTranspose.SelectedItemPosition != 0)
                {

                        m_spinnerTranspose.SetSelection(0);
                        AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                        dlMessage.SetTitle("Feature not enabled");
                        dlMessage.SetMessage("This feature is not enabled in the trial version of TTtuner. The full version can be purchased in the Google Play Store.");
                        dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                        dlMessage.Show();

                }

#endif



            };

            spinnerSampleRate = FindViewById<Spinner>(Resource.Id.spinnerSampleRate);

            var adapter1 = ArrayAdapter.CreateFromResource(
            this, Resource.Array.SampleRate_array, Resource.Layout.spinner_item);

            adapter1.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinnerSampleRate.Adapter = adapter1;

            spinnerSampleRate.ItemSelected += (sender, args) =>
            {
                string strSampleRate1;
                string[] strArrSampleRate = Common.StringResourceHelper.SampleRateArrayResource_Get(this);

                Common.Settings.SampleRate = spinnerSampleRate.Adapter.GetItem(args.Position).ToString();

                strSampleRate1 = spinnerSampleRate.Adapter.GetItem(args.Position).ToString();


            };

            // AudioPlayer
            spinnerAudioPlayer = FindViewById<Spinner>(Resource.Id.spinnerAudioPlayer);

            adapter1 = ArrayAdapter.CreateFromResource(
            this, Resource.Array.AudioPlayer_array, Resource.Layout.spinner_item);

            adapter1.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinnerAudioPlayer.Adapter = adapter1;

            spinnerAudioPlayer.ItemSelected += (sender, args) =>
            {
                string strAudioPlayer;
                string[] strArrSampleRate = Common.StringResourceHelper.AudioPlayerArrayResource_Get(this);

                Common.Settings.AudioPlayer = spinnerAudioPlayer.Adapter.GetItem(args.Position).ToString();

                strAudioPlayer = spinnerAudioPlayer.Adapter.GetItem(args.Position).ToString();
            };

            LoadSettingsDisplayValues();

        }

        public void OnTextNoteDetectionClarityClicked(object sender, EventArgs args)
        {
            var dialog = PopUpWithSeekerDialog.NewInstance(0, 1, 0.1, Common.Settings.NoteClarityFloat);
            dialog.Title = "Note Detection Clarity";
            dialog.Description = "This setting specifies the required clarity of played notes to be registered as a note by TTtuner";

            dialog.ValueSet += (sender2, value) =>
            {
                Common.Settings.NoteClarity = value;
                textNoteDetectionClarity.Text = value;
            };

            dialog.Show(SupportFragmentManager, "dialog");
        }



        protected void LoadSettingsDisplayValues()
        {
            string strTuningSystem, strScaleJustIntonation;

            strTuningSystem = Common.Settings.TuningSytem;
            strScaleJustIntonation = Common.Settings.RootScale;

            PackageInfo pInfo = this.PackageManager.GetPackageInfo(PackageName, 0);
            String strVersion = pInfo.VersionName;
#if Trial_Version
            strVersion += " Trial";
#else
            strVersion += " Full";
#endif

            textVersionNumber.Text = "TTtuner Version " + strVersion;

            textDataFolder.Text = Common.Settings.DataFolder;

            textTuningSystem.Text = strTuningSystem == "Equal Temperament" ? strTuningSystem : strTuningSystem + " ( " + strScaleJustIntonation + " )";

            textA4ref.Text = Common.Settings.A4ref + " Hz";

            textPitchSamplingFreq.Text = Common.Settings.PitchSmaplingFrequency + " Hz";

            textTimeLAg.Text = Common.Settings.TimeLagString;

            string strTRanspose = Common.Settings.Transpose;

            for (int i = 0; i < m_spinnerTranspose.Adapter.Count; i++)
            {
                var item = m_spinnerTranspose.Adapter.GetItem(i);
                if (strTRanspose == item.ToString())
                {
                    m_spinnerTranspose.SetSelection(i);

                    break;
                }

            }

            string strSampleRate = Common.Settings.SampleRate;

            for (int i = 0; i < spinnerSampleRate.Adapter.Count; i++)
            {
                var item = spinnerSampleRate.Adapter.GetItem(i);
                if (strSampleRate == item.ToString())
                {
                    spinnerSampleRate.SetSelection(i);
                    break;
                }

            }

            string strAudioPlayer = Common.Settings.AudioPlayer;

            for (int i = 0; i < spinnerAudioPlayer.Adapter.Count; i++)
            {
                var item = spinnerAudioPlayer.Adapter.GetItem(i);
                if (strAudioPlayer == item.ToString())
                {
                    spinnerAudioPlayer.SetSelection(i);
                    break;
                }
            }

            textMinNumberOfSamplesForNote.Text = Common.Settings.MinNumberOfSamplesForNote.ToString();
            textNoteDetectionClarity.Text = Common.Settings.NoteClarity;
        }

        protected void GraphConfigEventHandler(object sender, EventArgs args)
        {
            bool bVar ;
            var dialog = PopUpGraphConfiguration.NewInstance(
                this.Resources.GetString(Resource.String.GraphConfig_prompt),
                Common.Settings.GraphOverlay,
                (bool)Common.Settings.DisplayNotesGraph,
                (bool)Common.Settings.DisplayDecibelGraph,
                this);

            dialog.OkPressed += (s, strArr) =>
            {
                Common.Settings.DisplayNotesGraph = strArr[0] == "True" ? true : false;
                Common.Settings.DisplayDecibelGraph = strArr[1] == "True" ? true : false;
                Common.Settings.GraphOverlay = strArr[2];
            };

            dialog.Show(this.FragmentManager, "dialog");
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
        }

        void spinner_ItemSelected(object sender, EventArgs args)
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (lis1 != null)
            {
                lis1.FolderSelected -= FolderUpdate;
            }

        }
    }
}