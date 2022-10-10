using Android.Util;
using System;
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
                string path = Settings.DataFolder;
                string logFilePath = System.IO.Path.Combine(path, "log.txt");

                return logFilePath;
            }
        }
        static public void Info(string appName, string logText)
        {
            Log.Info(appName, logText);

            if (!WriteToLogcatOnly)
            {
                //using (System.IO.StreamWriter writer = new System.IO.StreamWriter(LOG_FILE_PATH, true))
                //{
                //    writer.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " : " + logText);
                //}
                builder.Append(DateTime.Now.ToString("hh.mm.ss.fff") + " : " + logText + "\n");

            }
        }

        static public void CreateLogFile()
        {
            if (!System.IO.File.Exists(LOG_FILE_PATH))
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(LOG_FILE_PATH, true))
                {
                    writer.WriteLine("Starting logging at " + System.DateTime.Now.ToString());
                }
            }
        }


        static public void DeleteLogFile()
        {
            if (System.IO.File.Exists(LOG_FILE_PATH))
            {
                System.IO.File.Delete(LOG_FILE_PATH);
            }
        }

        static public void FlushBufferToFile()
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(LOG_FILE_PATH, true))
            {
                writer.Write(builder.ToString());
            }
        }

    }
}