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
using System.Threading;
// using System.Xml.Linq;
using System.Xml;

namespace TTtuner_2022_2.Common
{
    internal partial class Settings
    {
        internal static void UpdateA4refButDontSavetoDisk(string stringVal)
        {
            m_strA4ref = stringVal;
            m_flA4ref = Convert.ToSingle(m_strA4ref);
            UpdateNotePitchMapStaticClass();
        }

        internal static void UpdateNotePitchMapStaticClass()
        {
            // update Notepitch map in the app

            if (m_intRootScaleOffset != int.MaxValue && m_intTranspositionOffset != int.MaxValue)
            {
                NotePitchMap.SetupTuningSystem(m_strTuningSystem, m_intRootScaleOffset, m_intTranspositionOffset, m_flA4ref);
            }
        }

        private static void WriteXmlSetting(string key, string strNewValue)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            try
            {
                doc.Load(filePath);

                //XmlNode root = doc.FirstChild;
                //root = root.NextSibling;

                XmlNodeList value = doc.GetElementsByTagName(key);

                value.Item(0).InnerText = strNewValue;

                doc.Save(filePath);


            }
            catch (System.IO.FileNotFoundException)
            {

            }


        }

        private static void WriteXmlSetting(string firstChild, string secondChild, string strNewValue)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(filePath);

                XmlNodeList firstChildList = doc.GetElementsByTagName(firstChild);

                XmlNode childNode = firstChildList.Item(0);

                foreach (var elem in childNode.ChildNodes)
                {

                    XmlNode childN = elem as XmlNode;
                    if (!childN.OuterXml.Contains("<" + firstChild + ">") && childN.OuterXml.Contains("<" + secondChild + ">"))
                    {
                        childN.Value = strNewValue;
                        doc.Save(filePath);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return;

        }

        private static string ReadXmlSetting(string key)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");
            XmlDocument doc = new XmlDocument();
            string retS = null;


            try
            {
                doc.Load(filePath);


                XmlNodeList value = doc.GetElementsByTagName(key);

                retS = value.Item(0).InnerText;
            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return retS;


        }

        private static string ReadXmlSetting(string firstChild, string secondChild)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");

            XmlDocument doc = new XmlDocument();
            string retS;


            try
            {
                doc.Load(filePath);


                XmlNodeList firstChildList = doc.GetElementsByTagName(firstChild);

                XmlNode childNode = firstChildList.Item(0);

                foreach (var elem in childNode.ChildNodes)
                {
                    XmlNode childN = elem as XmlNode;
                    if (!childN.OuterXml.Contains("<" + firstChild + ">") && childN.OuterXml.Contains("<" + secondChild + ">"))
                    {
                        return childN.InnerText;
                    }
                }

            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return "";
        }

        internal static void LoadAllSettings()
        {
            string strVal;

            strVal = Settings.RootScale;
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
            strVal = Settings.PositionOfPageAdapterOnMainActivity.ToString();
            strVal = Settings.LastColumnBrowsedOnStatsGrid;
            strVal = Settings.NoteClarity;
        }

        /// <summary>
        /// this function is for the case where you wish to commit the loaded settings to file 
        /// and load the values that are not present from the file
        /// At the end of the process both the loaded values and the values stored in the file will be 
        /// the same
        /// </summary>
        /// <param name="act"></param>
        internal static void MergeLoadedSettingsWithFile()
        {
            string strValue;

            // root scale
            if (string.IsNullOrEmpty(m_strRootScale))
            {
                // load the value from file if not present
                strValue = Settings.RootScale;
            }
            else
            {
                // save the value to file if it is
                Settings.RootScale = m_strRootScale;
            }

            // TimeLag 
            if (string.IsNullOrEmpty(m_strTimeLag))
            {
                strValue = Settings.TimeLagString;
            }
            else
            {
                Settings.TimeLagString = m_strTimeLag;
            }

            // Transpose 
            if (string.IsNullOrEmpty(m_strTransposition))
            {
                strValue = Settings.Transpose;
            }
            else
            {
                Settings.Transpose = m_strTransposition;
            }

            // TuningSytem 
            if (string.IsNullOrEmpty(m_strTuningSystem))
            {
                strValue = Settings.TuningSytem;
            }
            else
            {
                Settings.TuningSytem = m_strTuningSystem;
            }

            // PitchSmaplingFrequency 
            if (string.IsNullOrEmpty(m_strPitchSmaplingFrequency))
            {
                strValue = Settings.PitchSmaplingFrequency;
            }
            else
            {
                Settings.PitchSmaplingFrequency = m_strPitchSmaplingFrequency;
            }

            // SampleRate 
            if (string.IsNullOrEmpty(m_strSampleRate))
            {
                strValue = Settings.SampleRate;
            }
            else
            {
                Settings.SampleRate = m_strSampleRate;
            }

            // A4Ref 
            if (string.IsNullOrEmpty(m_strA4ref))
            {
                strValue = Settings.A4ref;
            }
            else
            {
                Settings.A4ref = m_strA4ref;
            }

            // datafolder 
            if (string.IsNullOrEmpty(m_strDataFolder))
            {
                strValue = Settings.DataFolder;
            }
            else
            {
                Settings.DataFolder = m_strDataFolder;
            }

            // audio player 
            if (string.IsNullOrEmpty(m_strAudioPlayer))
            {
                strValue = Settings.AudioPlayer;
            }
            else
            {
                Settings.AudioPlayer = m_strAudioPlayer;
            }

            // MinNumberOfSamplesForNote
            if (m_intMinNumberOfSamplesForNote == int.MaxValue)
            {
                strValue = Settings.MinNumberOfSamplesForNote.ToString();
            }
            else
            {
                Settings.MinNumberOfSamplesForNote = m_intMinNumberOfSamplesForNote;
            }


            // NotesGraph
            if (m_blDisplayNotesGraph == null)
            {
                strValue = Settings.DisplayNotesGraph.ToString();
            }
            else
            {
                Settings.DisplayNotesGraph = m_blDisplayNotesGraph;
            }

            // Decibel
            if (m_blDisplayDecibelGraph == null)
            {
                strValue = Settings.DisplayDecibelGraph.ToString();
            }
            else
            {
                Settings.DisplayDecibelGraph = m_blDisplayDecibelGraph;
            }

            //Graph overlay

            if (m_strGraphOverlay == null)
            {
                strValue = Settings.GraphOverlay;
            }
            else
            {
                Settings.GraphOverlay = m_strGraphOverlay;
            }

            // Freq Graph

            if (m_blSnapToNote_FreqGraph == null)
            {
                strValue = Settings.SnapToNote_Freq.ToString();
            }
            else
            {
                Settings.SnapToNote_Freq = m_blSnapToNote_FreqGraph;
            }

            if (m_blDefaultZoom_FreqGraph == null)
            {
                strValue = Settings.DefaultZoom_Freq.ToString();
            }
            else
            {
                Settings.DefaultZoom_Freq = m_blDefaultZoom_FreqGraph;
            }

            if (m_intZoomXms_FreqGraph == int.MaxValue)
            {
                strValue = Settings.ZoomXms_Freq.ToString();
            }
            else
            {
                Settings.ZoomXms_Freq = m_intZoomXms_FreqGraph;
            }

            if (m_intZoomY_FreqGraph == int.MaxValue)
            {
                strValue = Settings.ZoomY_Freq.ToString();
            }
            else
            {
                Settings.ZoomY_Freq = m_intZoomY_FreqGraph;
            }

            // Db Graph

            if (m_blSnapToNote_DbGraph == null)
            {
                strValue = Settings.SnapToNote_Db.ToString();
            }
            else
            {
                Settings.SnapToNote_Db = m_blSnapToNote_DbGraph;
            }

            if (m_blDefaultZoom_DbGraph == null)
            {
                strValue = Settings.DefaultZoom_Db.ToString();
            }
            else
            {
                Settings.DefaultZoom_Db = m_blDefaultZoom_DbGraph;
            }

            if (m_intZoomXms_DbGraph == int.MaxValue)
            {
                strValue = Settings.ZoomXms_Db.ToString();
            }
            else
            {
                Settings.ZoomXms_Db = m_intZoomXms_DbGraph;
            }

            if (m_intZoomY_DbGraph == int.MaxValue)
            {
                strValue = Settings.ZoomY_Db.ToString();
            }
            else
            {
                Settings.ZoomY_Db = m_intZoomY_DbGraph;
            }

            // PositionOfPageAdapterOnMainActivity
            if (m_intPositionOfPageAdapterOnMainActivity == int.MaxValue)
            {
                strValue = Settings.PositionOfPageAdapterOnMainActivity.ToString();
            }
            else
            {
                Settings.PositionOfPageAdapterOnMainActivity = m_intPositionOfPageAdapterOnMainActivity;
            }

            if (m_strLastColumnBrowsedOnStatsGrid == null)
            {
                strValue = Settings.LastColumnBrowsedOnStatsGrid;
            }
            else
            {
                Settings.LastColumnBrowsedOnStatsGrid = m_strLastColumnBrowsedOnStatsGrid;
            }

            //Note Clarity
            if (m_strNoteClarity == null)
            {
                strValue = Settings.NoteClarity;
            }
            else
            {
                Settings.NoteClarity = m_strNoteClarity;
            }


        }

        internal static void FlushAllUnsavedSettingsToDisk()
        {
            if (ReadXmlSetting("PositionOfPageAdapterOnMainActivity") != m_intPositionOfPageAdapterOnMainActivity.ToString())
            {
                WriteXmlSetting("PositionOfPageAdapterOnMainActivity", m_intPositionOfPageAdapterOnMainActivity.ToString());
            }
            if (ReadXmlSetting("LastColumnBrowsedOnStatsGrid") != m_strLastColumnBrowsedOnStatsGrid)
            {
                WriteXmlSetting("LastColumnBrowsedOnStatsGrid", m_strLastColumnBrowsedOnStatsGrid);
            }
        }

        internal static void WriteXMLFileFromManifestToInternalStorage(Activity act)
        {
            string resource = m_namespaceType.Namespace + ".Settings.xml";

            // check if file exits
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");



            using (Java.IO.File fl1 = new Java.IO.File(filePath))
            {

                using (var stream = m_namespaceType.Assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(reader.ReadToEnd());
                    doc.Save(fl1.Path);
                    reader.Close();
                }

                // file doesn't exist
            }
            Settings.CreateAndCorrectDataFolder(act);
        }

        internal static void ResetAndReloadSettings(Activity act)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");

            WriteXMLFileFromManifestToInternalStorage(act);

            m_strTimeLag = null;
            m_dblTimeLag = 0;
            m_strTuningSystem = null;
            m_strA4ref = null;
            m_flA4ref = 0;
            m_strPitchSmaplingFrequency = null;
            m_intPitchSmaplingPeriod = 0;
            m_strRootScale = null;
            m_intRootScaleOffset = int.MaxValue;
            m_strTransposition = null;
            m_intTranspositionOffset = int.MaxValue;
            m_strSampleRate = null;
            m_strDataFolder = null;
            m_strGraphOverlay = null;
            m_strLastColumnBrowsedOnStatsGrid = null;
            m_intMinNumberOfSamplesForNote = int.MaxValue;
            m_strNoteClarity = null;

            Common.Settings.CreateAndCorrectDataFolder(act);

            LoadAllSettings();

            UpdateNotePitchMapStaticClass();

        }

        internal static void CreateAndCorrectDataFolder(Activity act)
        {
            // this is only called when the xml file has been loaded from the manifest
            // make sure that the /storage/emulated/0/ is correct
            string strVal;
            string externalPath = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            strVal = Settings.DataFolder;

            if (!strVal.Contains(externalPath))
            {
                // set to default
                Settings.DataFolder = Path.Combine(externalPath, "TTtuner");
            }
            // Create Dir
            FileHelper.CreateDefaultDirectory(act);
        }

        internal static bool SanityCheckSettings()
        {
            // can add more sanity check here 
            // for now just check that tuning system is sane

            try
            {

                if (string.IsNullOrEmpty(Settings.A4ref))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(Settings.DataFolder))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(ReadXmlSetting("AudioPlayer")))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(ReadXmlSetting("GraphOverlay")))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(ReadXmlSetting("DecibelGraph")))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ReadXmlSetting("PositionOfPageAdapterOnMainActivity")))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ReadXmlSetting("LastColumnBrowsedOnStatsGrid")))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(ReadXmlSetting("NoteClarity")))
                {
                    return false;
                }
            }
            catch (Exception e1)
            {
                // issue with reading xml file
                // it must be deleted 
                return false;
            }

            return true;

        }

        internal static void LoadTuningSytems(Activity act)
        {
            string strFileText;
            double dbVal;


            strFileText = FileHelper.LoadText(act, Settings.TuningSystemsCsvFileName, true, CommonFunctions.TEXT_EXTENSION);
            var styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowDecimalPoint;

            var fmt = new NumberFormatInfo();
            fmt.NegativeSign = "-";
            // fmt.
            m_lstTuningSystems.Clear();

            if (string.IsNullOrEmpty(strFileText))
            {

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "the tuning system file is empty");
#endif
                throw new Exception("Tuning systems are empty");
            }

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "Here is the tuning system file:" + strFileText);
#endif

            string[] arrLines = strFileText.Split('\n');

            // ignore the first line
            for (int i = 1; i < arrLines.Count(); i++)
            {
                TuningSystem tsNew = new TuningSystem();
                List<double> lsNew = new List<double>();
                string[] arrFields = arrLines[i].Split('|');
                tsNew.Name = arrFields[0];
                for (int j = 1; j < arrFields.Count(); j++)
                {
                    if (!double.TryParse(arrFields[j], styles, CultureInfo.InvariantCulture, out dbVal))
                    {
                        Toast.MakeText(act, "Issue converting to double : arrFields[" + j + "] is " + arrFields[j], ToastLength.Long).Show();
                        return;
                    }
                    lsNew.Add(Convert.ToDouble(arrFields[j], CultureInfo.InvariantCulture));
                }
                tsNew.lstCentsDeviation = lsNew;
                m_lstTuningSystems.Add(tsNew);
            }

            //check that the tuning system is contained  in the list

            int matches = m_lstTuningSystems.Where(item => item.Name == Settings.TuningSytem).Count();
            if (matches != 1)
            {
                Settings.TuningSytem = m_lstTuningSystems[0].Name;
            }

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "The number of tuning systems loaded: " + m_lstTuningSystems.Count);
#endif

        }

        internal static void WriteCsvFileFromManifestToDataFolder(Activity activity)
        {
            string resource = m_namespaceType.Namespace + "." + Settings.TuningSystemsCsvFileName;
            string result;
            // check if file exits
            var documentsPath = Settings.DataFolder;
            var filePath = System.IO.Path.Combine(documentsPath, Settings.TuningSystemsCsvFileName);
            // make sure that the default directory is created

            FileHelper.CreateDefaultDirectory(activity);

            using (Java.IO.File fl1 = new Java.IO.File(filePath))
            {
                try
                {
                    using (var stream = m_namespaceType.Assembly.GetManifestResourceStream(resource))
                    {
                        using (FileStream fileStream = System.IO.File.Create(filePath, (int)stream.Length))
                        {
                            // Initialize the bytes array with the stream length and then fill it with data

#if Release_LogOutput
                            Logger.Info(Common.CommonFunctions.APP_NAME, "WriteCsvFileFromManifestToDataFolder, the number of bytes in the stream are" + (int)stream.Length);

#endif
                            byte[] bytesInStream = new byte[stream.Length];
                            stream.Read(bytesInStream, 0, bytesInStream.Length);
                            // Use write method to write to the file specified above
                            fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                        }
                    }
                }
                catch (Exception e1)
                {
                    Toast.MakeText(activity, "Problem writing csv file to data folder :" + documentsPath, ToastLength.Long).Show();

                }
                // file doesn't exist
            }
        }

        internal static string GetCsvFileTextFromManifest(Activity activity)
        {
            string resource = m_namespaceType.Namespace + "." + Settings.TuningSystemsCsvFileName;
            string result;
            try
            {
                using (var stream = m_namespaceType.Assembly.GetManifestResourceStream(resource))
                {
                    // Initialize the bytes array with the stream length and then fill it with data
                    byte[] bytesInStream = new byte[stream.Length];
                    stream.Read(bytesInStream, 0, bytesInStream.Length);
                    var str = System.Text.Encoding.Default.GetString(bytesInStream);
                    return str;
                }
            }
            catch (Exception e1)
            {
                Toast.MakeText(activity, "Problem reading data from tuning systems csv in manifest", ToastLength.Long).Show();

            }
            return null;
        }
    }
}