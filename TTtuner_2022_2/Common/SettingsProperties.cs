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
using System.IO;
using TTtuner_2022_2.Music;
using System.Reflection;
using Android.Provider;
using Android.Database;
using Java.IO;
using System.Globalization;
using Java.Nio.Channels;
using TTtuner_2022_2.Plot;
using System.Collections.ObjectModel;

namespace TTtuner_2022_2.Common
{
    internal partial class Settings
    {
        private static string m_strTimeLag = null;
        private static double m_dblTimeLag;
        private static string m_strTuningSystem = null;
        private static string m_strA4ref = null;
        private static float m_flA4ref = 0;
        private static string m_strPitchSmaplingFrequency = null;
        private static int m_intPitchSmaplingPeriod = 0;
        private static string m_strRootScale = null;
        private static int m_intRootScaleOffset = int.MaxValue;
        private static string m_strTransposition = null;
        private static int m_intTranspositionOffset = int.MaxValue;
        private static string m_strSampleRate = null;
        private static string m_strDataFolder = null;
        private static string m_strAudioPlayer = null;
        private static int m_intMinNumberOfSamplesForNote = int.MaxValue;
        private static bool? m_blDisplayNotesGraph = null;
        private static bool? m_blDisplayDecibelGraph = null;
        private static string m_strGraphOverlay = null;

        private static bool? m_blSnapToNote_FreqGraph = null;
        private static bool? m_blDefaultZoom_FreqGraph = null;
        private static int m_intZoomXms_FreqGraph = int.MaxValue;
        private static int m_intZoomY_FreqGraph = int.MaxValue;

        private static bool? m_blSnapToNote_DbGraph = null;
        private static bool? m_blDefaultZoom_DbGraph = null;
        private static int m_intZoomXms_DbGraph = int.MaxValue;
        private static int m_intZoomY_DbGraph = int.MaxValue;

        private static string m_strNoteClarity = null;
        private static float m_flNoteClarity = float.MaxValue;

        private static int m_intPositionOfPageAdapterOnMainActivity = int.MaxValue;

        private static string m_strLastColumnBrowsedOnStatsGrid = null;

        private static Type m_namespaceType = null;
        private static Activity m_act;
        private static List<TuningSystem> m_lstTuningSystems = new List<TuningSystem>();


        internal static string A4ref
        {
            get
            {
                if (m_strA4ref == null)
                {
                    // read from xmlFile
                    m_strA4ref = ReadXmlSetting("A4reference");
                    m_flA4ref = Convert.ToSingle(m_strA4ref);
                }
                return m_strA4ref;
            }

            set
            {
                //save to disk
                WriteXmlSetting("A4reference", value);
                // do everything else
                UpdateA4refButDontSavetoDisk(value);
            }
        }

        internal static double A4refDouble
        {
            get
            {
                return m_flA4ref;
            }
        }

        internal static string AudioPlayer
        {
            get
            {
                if (m_strAudioPlayer == null)
                {
                    // read from xmlFile

                    m_strAudioPlayer = ReadXmlSetting("AudioPlayer");

                }
                return m_strAudioPlayer;
            }
            set
            {
                m_strAudioPlayer = value;

                WriteXmlSetting("AudioPlayer", value);
            }
        }

        internal static bool? DisplayNotesGraph
        {
            get
            {
                if (m_blDisplayNotesGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DisplayNotesGraph");
                    m_blDisplayNotesGraph = strVal == "True" ? true : false;
                }

                return m_blDisplayNotesGraph;
            }

            set
            {
                m_blDisplayNotesGraph = value;

                WriteXmlSetting("DisplayNotesGraph", (bool)m_blDisplayNotesGraph ? "True" : "False");
            }
        }

        internal static bool? DisplayDecibelGraph
        {
            get
            {
                if (m_blDisplayDecibelGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DisplayDecibelGraph");
                    m_blDisplayDecibelGraph = strVal == "True" ? true : false;
                }

                return m_blDisplayDecibelGraph;
            }

            set
            {
                m_blDisplayDecibelGraph = value;

                WriteXmlSetting("DisplayDecibelGraph", (bool)m_blDisplayDecibelGraph ? "True" : "False");
            }
        }

        internal static string DataFolder
        {
            get
            {

                if (m_strDataFolder == null)
                {
                    // read from xmlFile

                    m_strDataFolder = ReadXmlSetting("DataFolder");

                }

                return m_strDataFolder;
            }
            set
            {
                m_strDataFolder = value;

                WriteXmlSetting("DataFolder", value);
            }
        }

        internal static string GraphOverlay
        {
            get
            {
                if (m_strGraphOverlay == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("GraphOverlay");
                    m_strGraphOverlay = strVal;
                }

                return m_strGraphOverlay;
            }

            set
            {
                m_strGraphOverlay = value;

                WriteXmlSetting("GraphOverlay", m_strGraphOverlay);
            }
        }

        internal static string LastColumnBrowsedOnStatsGrid
        {
            get
            {
                if (m_strLastColumnBrowsedOnStatsGrid == null)
                {
                    m_strLastColumnBrowsedOnStatsGrid = ReadXmlSetting("LastColumnBrowsedOnStatsGrid");
                }
                return m_strLastColumnBrowsedOnStatsGrid;
            }
            set
            {
                // this value is only saved in memory. Figured that to write to disk everytime 
                // may be excisive as this setting will change a lot
                m_strLastColumnBrowsedOnStatsGrid = value;
            }
        }

        internal static int MinNumberOfSamplesForNote
        {
            get
            {
                if (m_intMinNumberOfSamplesForNote == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("MinNumberOfSamplesForNote");
                    m_intMinNumberOfSamplesForNote = Convert.ToInt32(strVal);
                }

                return m_intMinNumberOfSamplesForNote;
            }

            set
            {
                m_intMinNumberOfSamplesForNote = value;

                WriteXmlSetting("MinNumberOfSamplesForNote", m_intMinNumberOfSamplesForNote.ToString());
            }
        }

        internal static List<string> TuningSystemsList
        {
            get
            {
                return m_lstTuningSystems.Select(item => item.Name).OrderBy(item => item).ToList();
            }
        }

        internal static int NumberOfSamplesInBuffer
        {
            get
            {
                int intPitchSmaplingPeriod;

                intPitchSmaplingPeriod = (int)Math.Floor(1d / Convert.ToDouble(m_strPitchSmaplingFrequency, CultureInfo.InvariantCulture) * 1000);
                // The number of samples should never be less than 400 or more than 1024 for pitch accurary / efficiency
                int val = (int)Math.Max(400.0f, (SampleRateInt * intPitchSmaplingPeriod * 0.001f));
                return Math.Min(val, 1024);
            }
        }

        internal static string NoteClarity
        {
            get
            {
                if (m_strNoteClarity == null)
                {
                    m_strNoteClarity = ReadXmlSetting("NoteClarity");
                    m_flNoteClarity = Convert.ToSingle(m_strNoteClarity, CultureInfo.InvariantCulture);
                }
                return m_strNoteClarity;
            }

            set
            {
                m_strNoteClarity = value;
                m_flNoteClarity = Convert.ToSingle(m_strNoteClarity, CultureInfo.InvariantCulture);
                WriteXmlSetting("NoteClarity", value);
            }
        }

        internal static float NoteClarityFloat
        {
            get
            {
                return m_flNoteClarity;
            }
        }

        internal static string SampleRate
        {
            get
            {

                if (m_strSampleRate == null)
                {
                    // read from xmlFile
                    m_strSampleRate = ReadXmlSetting("SampleRate");
                }

                return m_strSampleRate;
            }

            set
            {
                CommonFunctions comFunc = new CommonFunctions();

                m_strSampleRate = value;
                WriteXmlSetting("SampleRate", value);

            }
        }

        internal static int SampleRateInt
        {
            get
            {
                return Convert.ToInt32(m_strSampleRate);
            }
        }

        internal static string PitchSmaplingFrequency
        {
            get
            {

                if (m_strPitchSmaplingFrequency == null)
                {
                    // read from xmlFile

                    m_strPitchSmaplingFrequency = ReadXmlSetting("PitchSmaplingFrequency");
                }

                return m_strPitchSmaplingFrequency;
            }

            set
            {

                m_strPitchSmaplingFrequency = value;
                WriteXmlSetting("PitchSmaplingFrequency", value);
            }
        }

        internal static int PitchSamplingFrequencyInt
        {
            get
            {
                return (int)((1 / (float)PitchSmaplingPeriodMs) * 1000);
            }
        }

        internal static int PitchSmaplingPeriodMs
        {
            get
            {
                int intSamplingWindowMs = (int)(NumberOfSamplesInBuffer / (float)SampleRateInt * 1000);
                m_intPitchSmaplingPeriod = (int)Math.Floor(1d / Convert.ToDouble(m_strPitchSmaplingFrequency, CultureInfo.InvariantCulture) * 1000);

                // the period should be between 10 and 50 ms
                m_intPitchSmaplingPeriod = Math.Max(10, m_intPitchSmaplingPeriod);
                m_intPitchSmaplingPeriod = Math.Min(50, m_intPitchSmaplingPeriod);

                // if the period is below the sampling window width than return the sample window width
                if (m_intPitchSmaplingPeriod < intSamplingWindowMs)
                {
                    return intSamplingWindowMs;
                }
                else
                {
                    return m_intPitchSmaplingPeriod;
                }
            }
        }

        internal static int PositionOfPageAdapterOnMainActivity
        {
            get
            {
                if (m_intPositionOfPageAdapterOnMainActivity == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("PositionOfPageAdapterOnMainActivity");
                    m_intPositionOfPageAdapterOnMainActivity = Convert.ToInt32(strVal);
                }
                return m_intPositionOfPageAdapterOnMainActivity;
            }
            set
            {
                // this value is only saved in memory. Figured that to write to disk everytime 
                // may be excisive as this setting will change a lot
                m_intPositionOfPageAdapterOnMainActivity = value;

            }
        }

        internal static string RootScale
        {
            get
            {
                CommonFunctions comFunc = new CommonFunctions();

                if (m_strRootScale == null)
                {
                    // read from xmlFile

                    m_strRootScale = ReadXmlSetting("RootScale");
                    m_intRootScaleOffset = comFunc.GetIndexOfItemInstringArray(StringResourceHelper.ScalesArrayResource_Get(m_act), m_strRootScale);

                }
                return m_strRootScale;
            }

            set
            {
                CommonFunctions comFunc = new CommonFunctions();

                m_strRootScale = value;
                m_intRootScaleOffset = comFunc.GetIndexOfItemInstringArray(StringResourceHelper.ScalesArrayResource_Get(m_act), m_strRootScale);

                WriteXmlSetting("RootScale", value);

                UpdateNotePitchMapStaticClass();
            }
        }

        internal static int RootScaleOffset
        {
            get
            {
                string strVal;
                if (m_intRootScaleOffset == int.MaxValue)
                {
                    strVal = RootScale;
                }

                return m_intRootScaleOffset;
            }
        }

        internal static string TuningSytem
        {
            get
            {
                if (m_strTuningSystem == null)
                {
                    // read from xmlFile

                    m_strTuningSystem = ReadXmlSetting("TuningSystem");
                }

                return m_strTuningSystem;
            }

            set
            {
                m_strTuningSystem = value;

                WriteXmlSetting("TuningSystem", value);

                UpdateNotePitchMapStaticClass();

            }
        }

        internal static string TuningSystemAndRootScale
        {
            get
            {

                if (TuningSytem != "Equal Temperament")
                {
                    return TuningSytem + " (" + RootScale + ")";
                }
                else
                {
                    return TuningSytem;
                }
            }
        }

        internal static string Transpose
        {
            get
            {
                CommonFunctions comFunc = new CommonFunctions();

                if (m_strTuningSystem == null)
                {
                    // read from xmlFile

                    m_strTransposition = ReadXmlSetting("Transpose");
                    m_intTranspositionOffset = comFunc.GetIndexOfItemInstringArray(StringResourceHelper.TransposeArrayResource_Get(m_act), m_strTransposition);

                }

                return m_strTransposition;
            }
            set
            {
                CommonFunctions comFunc = new CommonFunctions();

                m_strTransposition = value;
                m_intTranspositionOffset = comFunc.GetIndexOfItemInstringArray(StringResourceHelper.TransposeArrayResource_Get(m_act), m_strTransposition);

                WriteXmlSetting("Transpose", value);

                UpdateNotePitchMapStaticClass();
            }
        }

        internal static int TransposeOffset
        {
            get
            {
                string strVal;
                if (m_intTranspositionOffset == int.MaxValue)
                {
                    strVal = Transpose;
                }

                return m_intTranspositionOffset;
            }
        }

        internal static string TimeLagString
        {
            get
            {

                if (m_strTimeLag == null)
                {
                    // read from xmlFile

                    m_strTimeLag = ReadXmlSetting("TimeLag");
                    m_dblTimeLag = Convert.ToDouble(m_strTimeLag, CultureInfo.InvariantCulture) / 1000;

                }

                return m_strTimeLag;
            }

            set
            {
                CommonFunctions comFunc = new CommonFunctions();

                m_strTimeLag = value;
                m_dblTimeLag = Convert.ToDouble(m_strTimeLag, CultureInfo.InvariantCulture) / 1000;
                WriteXmlSetting("TimeLag", value);

            }
        }

        internal static double TimeLagSec
        {
            get
            {
                return m_dblTimeLag;
            }
        }

        internal static List<double> TuningSystemCentsDeviation
        {
            get
            {
                return m_lstTuningSystems.Where(item => item.Name == TuningSytem).FirstOrDefault().lstCentsDeviation;
            }
        }

        internal static bool? SnapToNote_Freq
        {
            get
            {
                if (m_blSnapToNote_FreqGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("FrequencyGraph", "SnapToNote");
                    m_blSnapToNote_FreqGraph = strVal == "True" ? true : false;
                }

                return m_blSnapToNote_FreqGraph;
            }

            set
            {
                m_blSnapToNote_FreqGraph = value;

                WriteXmlSetting("FrequencyGraph", "SnapToNote", (bool)m_blSnapToNote_FreqGraph ? "True" : "False");
            }
        }

        internal static bool? DefaultZoom_Freq
        {
            get
            {
                if (m_blDefaultZoom_FreqGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("FrequencyGraph", "DefaultZoom");
                    m_blDefaultZoom_FreqGraph = strVal == "True" ? true : false;
                }

                return m_blDefaultZoom_FreqGraph;
            }

            set
            {
                m_blDefaultZoom_FreqGraph = value;

                WriteXmlSetting("FrequencyGraph", "DefaultZoom", (bool)m_blDefaultZoom_FreqGraph ? "True" : "False");
            }
        }

        internal static int ZoomXms_Freq
        {
            get
            {
                if (m_intZoomXms_FreqGraph == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("FrequencyGraph", "ZoomLevelXms");
                    m_intZoomXms_FreqGraph = Convert.ToInt32(strVal);
                }

                return m_intZoomXms_FreqGraph ;
            }

            set
            {
                m_intZoomXms_FreqGraph = (int) (value);

                WriteXmlSetting("FrequencyGraph", "ZoomLevelXms", m_intZoomXms_FreqGraph.ToString());
            }
        }

        internal static float ZoomY_Freq
        {
            get
            {
                if (m_intZoomY_FreqGraph == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("FrequencyGraph", "ZoomLevelY");
                    m_intZoomY_FreqGraph = Convert.ToInt32(strVal);
                }

                return m_intZoomY_FreqGraph / 100f;
            }

            set
            {
                m_intZoomY_FreqGraph = (int)(value * 100);

                WriteXmlSetting("FrequencyGraph", "ZoomLevelY", m_intZoomY_FreqGraph.ToString());
            }
        }

        internal static bool? SnapToNote_Db
        {
            get
            {
                if (m_blSnapToNote_DbGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DecibelGraph", "SnapToNote");
                    m_blSnapToNote_DbGraph = strVal == "True" ? true : false;
                }

                return m_blSnapToNote_DbGraph;
            }

            set
            {
                m_blSnapToNote_DbGraph = value;

                WriteXmlSetting("DecibelGraph", "SnapToNote", (bool)m_blSnapToNote_DbGraph ? "True" : "False");
            }
        }

        internal static bool? DefaultZoom_Db
        {
            get
            {
                if (m_blDefaultZoom_DbGraph == null)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DecibelGraph", "DefaultZoom");
                    m_blDefaultZoom_DbGraph = strVal == "True" ? true : false;
                }

                return m_blDefaultZoom_DbGraph;
            }

            set
            {
                m_blDefaultZoom_DbGraph = value;

                WriteXmlSetting("DecibelGraph", "DefaultZoom", (bool)m_blDefaultZoom_DbGraph ? "True" : "False");
            }
        }

        internal static int ZoomXms_Db
        {
            get
            {
                if (m_intZoomXms_DbGraph == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DecibelGraph", "ZoomLevelXms");
                    m_intZoomXms_DbGraph = Convert.ToInt32(strVal);
                }

                return m_intZoomXms_DbGraph;
            }

            set
            {
                m_intZoomXms_DbGraph = (int)(value);

                WriteXmlSetting("DecibelGraph", "ZoomLevelXms", m_intZoomXms_DbGraph.ToString());
            }
        }

        internal static float ZoomY_Db
        {
            get
            {
                if (m_intZoomY_DbGraph == int.MaxValue)
                {
                    string strVal;
                    strVal = ReadXmlSetting("DecibelGraph", "ZoomLevelY");
                    m_intZoomY_DbGraph = Convert.ToInt32(strVal);
                }

                return m_intZoomY_DbGraph / 100f;
            }

            set
            {
                m_intZoomY_DbGraph = (int)(value * 100);

                WriteXmlSetting("DecibelGraph", "ZoomLevelY", m_intZoomY_DbGraph.ToString());
            }
        }

        internal static void Init(Type type, Activity act)
        {
            m_namespaceType = type;
            m_act = act;
        }


        internal class TuningSystem
        {
            internal string Name;
            internal List<double> lstCentsDeviation = new List<double>();
        }


    }
}