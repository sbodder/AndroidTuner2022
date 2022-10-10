using System.Reflection;
using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;
using TTtuner_2022_2;
using TTtuner_2022_2.Common;
using System;

namespace TTtunerUnitTests
{
    [Activity(Label = "TTtunerUnitTests", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            CommonFunctions cm = new CommonFunctions();
            Type type = this.GetType();
            // tests can be inside the main assembly
            AddTest(Assembly.GetExecutingAssembly());

            Settings.Init(type, this);

            MockSettings();

            // Setup settings from xml file
            cm.SetupXmlSettingsFile(this);
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }

        private void MockSettings()
        {
            string strVal;

            StringResourceHelper.Init(  new string[] { "A", "B", "D" },
                                        new string[] { "C (C->C)", "C# (C->B)" },
                                        new string[] { "8000", "11025" },
                                        new string[] { "TimeStretch", "audio player2" }
                                      );

            Settings.RootScale = "D";
            strVal = Settings.TimeLagString;
            strVal = Settings.Transpose;
            strVal = Settings.TuningSytem;
            strVal = Settings.PitchSmaplingFrequency;
            strVal = Settings.SampleRate;
            strVal = Settings.A4ref;
            strVal = Settings.DataFolder;
            strVal = Settings.AudioPlayer;
            strVal = Settings.MinNumberOfSamplesForNote.ToString();
            strVal = Settings.DisplayNotesGraph.ToString();
            strVal = Settings.DisplayDecibelGraph.ToString();
            strVal = Settings.GraphOverlay;
            strVal = Settings.SnapToNote_Freq.ToString();
            strVal = Settings.DefaultZoom_Freq.ToString();
            strVal = Settings.ZoomXms_Freq.ToString();
            strVal = Settings.ZoomY_Freq.ToString();
            strVal = Settings.SnapToNote_Db.ToString();
            strVal = Settings.DefaultZoom_Db.ToString();
            strVal = Settings.ZoomXms_Db.ToString();
            strVal = Settings.ZoomY_Db.ToString();
        }
    }
}

