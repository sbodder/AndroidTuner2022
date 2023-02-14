using Android.OS;
using Android.Util;
using System;
using System.IO;
using System.Text;

namespace TTtuner_2022_2.Common
{
    /// <summary>
    /// This class write to both the standard logcat and also a file in the datafolder (for users who can't access logcat)
    /// </summary>
    class Logger
    {
        static public bool WriteToLogcatOnly = false;

        static private StringBuilder builder = new StringBuilder("");


        static string LOG_FILE_PATH
        {
            get
            {
                string path;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    path = Settings.MediaStoreFolder;
 
                }
                else
                {
                    path = Settings.DataFolder;
                }

                string logFilePath = System.IO.Path.Combine(path, "log.txt");

                return logFilePath;
            }
        }
        static public void Info(string appName, string logText)
        {
            Log.Info(appName, logText);

            if (!WriteToLogcatOnly)
            {
                
                builder.Append(DateTime.Now.ToString("hh.mm.ss.fff") + " : " + logText + "\n");
            }
        }

        static public void CreateLogFile()
        {
            if (!FileHelper.CheckIfFileExists(LOG_FILE_PATH, false))
            {
                string strToWrite = "Starting logging at " + System.DateTime.Now.ToString();

                using (System.IO.Stream os = FileHelper.OpenFileOutputStream(LOG_FILE_PATH, false, strToWrite.Length, null, true))
                {

                    using (StreamWriter writer = new StreamWriter(os))
                    {
                        writer.Write(strToWrite);
                        writer.Close();
                        writer.DisposeAsync();
                    }
                }
            }
        }


        static public void DeleteLogFile()
        {
            if (FileHelper.CheckIfFileExists(LOG_FILE_PATH, false))
            {
                FileHelper.DeleteFile(LOG_FILE_PATH, false);
            }
        }

        static public void FlushBufferToFile()
        {
            using (System.IO.Stream os = FileHelper.OpenFileOutputStream(LOG_FILE_PATH, false, builder.ToString().Length, null, true))
            {

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(os))
                {
                    writer.Write(builder.ToString());
                }
            }

            builder.Clear();
        }

    }
}