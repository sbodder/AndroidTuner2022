using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Net;

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
using Plugin.CurrentActivity;
using Xamarin.Essentials;
using Android.Media;
using Java.Net;
using Stream = System.IO.Stream;
using File = System.IO.File;

namespace TTtuner_2022_2.Common
{
    internal static class FileHelper
    {

        static bool mExternalStorageAvailable = false;
        static bool mExternalStorageWriteable = false;

        internal static void DeleteFile(string sourcePath)
        {
            // delete the  file
            Java.IO.File fl = new Java.IO.File(sourcePath);
            if (fl.Exists())
            {
                fl.Delete();
            }

            fl.Dispose();
        }

        internal static void CopyFile(string sourcePath, string destPath)
        {
            Java.IO.File fl1 = new Java.IO.File(sourcePath);
            Java.IO.File fl2 = new Java.IO.File(destPath);

            // copy file to new location
            FileChannel src = new FileInputStream(fl1).Channel;
            FileChannel dest = new FileOutputStream(fl2).Channel;

            dest.TransferFrom(src, 0, src.Size());

            fl1.Dispose();
            fl2.Dispose();
        }

        internal static Stream OpenFileInputStream(string fileName)
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                return MediaStoreHelper.OpenFileInputStream(fileName, "");
            }
            else
            {
                var filePath = System.IO.Path.Combine(Common.Settings.DataFolder, fileName);
                return File.Open(filePath, FileMode.Open);
            }


        }

        internal static Stream OpenFileOutputStream(string fileName, long lenghtInBytes)
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                return MediaStoreHelper.OpenFileOutputStream(fileName, lenghtInBytes);
            }
            else
            {
                var filePath = System.IO.Path.Combine(Common.Settings.DataFolder, fileName);
                return File.Open(filePath, FileMode.Create);
            }


        }

        internal static long GetLengthOfFile(string filePath)
        {
            Java.IO.File fl = new Java.IO.File(filePath);
            long lgLength = fl.Length();

            fl.Dispose();

            return lgLength;
        }

        internal static string DataDirectory
        {
            get
            {
                CheckExternalStorageAvailable();
                if (!mExternalStorageAvailable)
                {
                    return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                }
                else
                {
                    return Settings.DataFolder;
                }
            }
        }

        static internal void CheckExternalStorageAvailable()
        {

            String state = global::Android.OS.Environment.ExternalStorageState;

            if (global::Android.OS.Environment.MediaMounted == state)
            {
                mExternalStorageAvailable = mExternalStorageWriteable = true;
            }
            else if (global::Android.OS.Environment.MediaMountedReadOnly == state)
            {
                mExternalStorageAvailable = true;
                mExternalStorageWriteable = false;
            }
            else
            {
                mExternalStorageAvailable = mExternalStorageWriteable = false;
            }


        }


        static internal void CreateDefaultDirectory(Activity act)
        {

            CheckExternalStorageAvailable();

            if (!mExternalStorageWriteable)
            {
                Toast.MakeText(act, "Do not have access to external storage ", ToastLength.Long).Show();
                return;

            }
            else
            {
                // write to external storage
                var documentsPath = Common.Settings.DataFolder;

                try
                {
                    System.IO.Directory.CreateDirectory(documentsPath);
                }
                catch (Exception e1)
                {
                    Toast.MakeText(act, "Problem creating Directory " + documentsPath, ToastLength.Long).Show();
                }
            }
        }

        static internal FileStream CreateFile(Activity act, string filename, bool blExternal)
        {
            System.IO.FileStream os = null;
            CheckExternalStorageAvailable();

            if (!blExternal || !mExternalStorageWriteable)
            {
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, filename);
                os = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            }
            else
            {
                // write to external storage
                var documentsPath = Common.Settings.DataFolder;
                var filePath = Path.Combine(documentsPath, filename);

                try
                {
                    os = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                }
                catch (Exception e1)
                {
                    Toast.MakeText(act, "Problem creating File " + filePath, ToastLength.Long).Show();
                }
            }

            return os;



        }
        static internal void SaveTextFile(Activity act, string filename, string text, bool blExternal)
        {
            CheckExternalStorageAvailable();

            if (!blExternal || !mExternalStorageWriteable)
            {
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, filename);
                System.IO.File.WriteAllText(filePath, text);

            }
            else
            {
                // write to external storage
                var documentsPath = Common.Settings.DataFolder;
                var filePath = Path.Combine(documentsPath, filename);

                try
                {
                    System.IO.File.WriteAllText(filePath, text);
                }
                catch (Exception e1)
                {
                    Toast.MakeText(act, "Problem saving File " + filePath, ToastLength.Long).Show();
                }
            }

        }
        static internal string LoadText(Activity act, string filename, bool blExternal, string mediaStoreExtension = "")
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                return MediaStoreHelper.GetFileText(act, filename, mediaStoreExtension);
            }
            else
            {
                return LoadTextLegacy(act, filename, blExternal);
            }

        }

        static internal bool CheckIfFileExists(Activity act, string fileName, string mediaStoreFileExt)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                return MediaStoreHelper.CheckIfFileExists(act, fileName, mediaStoreFileExt);

            }
            else
            {
                // check if xml file exits
                var documentsPath = Settings.DataFolder;
                var filePath = System.IO.Path.Combine(documentsPath, fileName);

                using (Java.IO.File fl1 = new Java.IO.File(filePath))
                {
                    if (fl1.Exists())
                    {
                        return true;
                    }
                    // file doesn't exist
                }

                return false;
            }

        }


        static internal string LoadTextLegacy(Activity act, string filename, bool blExternal)
        {

            string strText = "";
            CheckExternalStorageAvailable();
            CommonFunctions comFunc = new CommonFunctions();

            //if (!mExternalStorageAvailable)
            if (!blExternal || !mExternalStorageAvailable)
            {
                var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, filename);
                try
                {
                    //  strText = System.IO.File.ReadAllText(filePath);

                    //Get the text file
                    Java.IO.File file = new Java.IO.File(filePath);

                    //Read text from file
                    StringBuilder text = new StringBuilder();

                    BufferedReader br = new BufferedReader(new FileReader(file));
                    String line;

                    while ((line = br.ReadLine()) != null)
                    {
                        text.Append(line);
                        text.Append('\n');
                    }
                    br.Close();

                    strText = text.ToString();
                }
                catch (Exception e1)
                {
                    Toast.MakeText(act, "Cannot find file " + filePath + " : " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();
                    throw e1;
                }

                return strText;
            }
            else
            {
                // READ FILE from folder specified in settings
                var documentsPath = Common.Settings.DataFolder;
                var filePath = Path.Combine(documentsPath, filename);
                try
                {
                    //  strText = System.IO.File.ReadAllText(filePath);

                    //Get the text file
                    Java.IO.File file = new Java.IO.File(filePath);

                    //Read text from file
                    StringBuilder text = new StringBuilder();

                    BufferedReader br = new BufferedReader(new FileReader(file));
                    String line;

                    while ((line = br.ReadLine()) != null)
                    {
                        text.Append(line);
                        text.Append('\n');
                    }
                    br.Close();

                    strText = text.ToString();
                }
                catch (Exception e1)
                {
                    Toast.MakeText(act, "Cannot find file " + filePath + " : " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();
                }

                return strText;
            }

        }


    }



}