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
using Android.Systems;
using Java.Security.Cert;
using Android.Util;
using System.Runtime.InteropServices.ComTypes;
using Android.Views.Inspectors;
using static Android.Provider.ContactsContract;

namespace TTtuner_2022_2.Common
{
    internal static class FileHelper
    {

        static bool mExternalStorageAvailable = false;
        static bool mExternalStorageWriteable = false;

        internal static void DeleteFile(string sourcePath, bool internalAppSpace = true)
        {
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(sourcePath);
            string filePath = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (internalAppSpace)
                {
                    filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
                }
                else
                {
                    MediaStoreHelper.DeleteFile(sourcePath);
                    return;
                }
            }
            else
            {
                filePath = sourcePath;
            }

            // delete the  file
            Java.IO.File fl = new Java.IO.File(filePath);
            if (fl.Exists())
            {
                fl.Delete();
            }
            fl.Dispose();
        }

        static internal IList<FileInfoItem> GetStatsFileInInternalAppSpace()
        {
            var dir = new DirectoryInfo(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
            IList <FileInfoItem> files = new List<FileInfoItem>();

            foreach (var item in dir.GetFileSystemInfos().Where(item =>  item.Extension.ToLower() == CommonFunctions.STAT_FILE_EXTENSION).OrderByDescending(s => s.Name))
            {
                files.Add(new FileInfoItem(item.Name, item.FullName));
            }

            return files;
        }

        static internal IList<FileInfoItem> GetMediaFileListInDirectory(string directory)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var list = MediaStoreHelper.GetMediaFilesInAppDirectory();
                var list2 = GetStatsFileInInternalAppSpace();
                // this is the only directory that will be useful to retrieve if on scoped storage

                foreach (var item in list2)
                {
                    list.Add(item);
                }
                return list.OrderBy(s => s.Name).ToList(); 
            }

            //legacy
            IList<FileInfoItem> visibleThings = new List<FileInfoItem>();
            var dir = new DirectoryInfo(directory);

            int i = 0;

            var items = dir.GetFileSystemInfos();
            try
            {
                foreach (var item in dir.GetFileSystemInfos()
                    .Where(item => item.Extension.ToLower() == CommonFunctions.WAV_FILE_EXTENSION || item.Extension.ToLower() == CommonFunctions.STAT_FILE_EXTENSION)
                    .OrderByDescending(s => s.Name))
                {
                    i++;
                    visibleThings.Add(new FileInfoItem(item.Name, item.FullName, ViewHelpers.IsDirectory(item)));
                }

                return visibleThings;

            }
            catch (Exception ex)
            {
#if Debug
                    Logger.Error("FileListFragment", "Couldn't access the directory " + _directory.FullName + "; " + ex);
                    Toast.MakeText(Activity, "Problem retrieving contents of " + directory, ToastLength.Long).Show();
#endif
                throw ex;

            }

        }

        internal static string GetFilePath(string filename, bool internalAppDir, string mimetype = null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (internalAppDir)
                {
                    return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
                }
                else
                {
                    return MediaStoreHelper.GetNewFilePath(filename);
                }
            }
            else
            {
                string strPersonalPath = DataDirectory;
                return System.IO.Path.Combine(strPersonalPath, filename);
            }
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


        internal static string CopyFileUriToInternalAppStorage(global::Android.Net.Uri uri)
        {

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            var filename = GetFileNameFromUri(uri);
            var newfilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);

            using (System.IO.Stream si = resolver.OpenInputStream(uri))
            {
                using (System.IO.Stream os = FileHelper.OpenFileOutputStream(newfilePath))
                {
                    si.CopyTo(os);
                }
            }
            return newfilePath;
        }

        internal static string GetFileNameFromUri(global::Android.Net.Uri uri)
        {
            String result = null;
            if (uri.Scheme == "content")
            {
                ICursor cursor = CrossCurrentActivity.Current.AppContext.ContentResolver.Query(uri, null, null, null, null);
                try
                {
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        result = cursor.GetString(cursor.GetColumnIndex(IOpenableColumns.DisplayName));
                    }
                }
                finally
                {
                    cursor.Close();
                }
            }
            if (result == null)
            {
                result = uri.Path;
                int cut = result.LastIndexOf('/');
                if (cut != -1)
                {
                    result = result.Substring(cut + 1);
                }
            }
            return result;
        }

        internal static Stream OpenFileInputStream(string filePath, bool internalAppSpace = true, string mimetype = null)
        {
            string newfilePath;
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filePath);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (internalAppSpace)
                {
                    newfilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
                }
                else
                {
                    return MediaStoreHelper.OpenFileInputStream(fileName, String.Empty, mimetype);

                }
            }
            else
            {
                newfilePath = System.IO.Path.Combine(Common.Settings.DataFolder, fileName);               
            }

            return File.Open(newfilePath, FileMode.Open);
        }

        internal static void CopyFileFromInternalStorageToScoped(string filePath)
        {
            CommonFunctions comF = new CommonFunctions();
            string fileName = comF.GetFileNameFromPath(filePath);

            using (Stream si = FileHelper.OpenFileInputStream(filePath))
            {
                using (Stream so = MediaStoreHelper.OpenFileOutputStream(fileName, -1, "audio/wav"))
                {
                    si.CopyTo(so);
                }
            }
        }

            internal static string CopyFileFromScopedStorageToInternal(string filePath)
        {
            Java.IO.File fl2;
            Stream si = null, os = null;
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filePath);

            try
            {
                si = MediaStoreHelper.OpenFileInputStream(filePath);   
                os = FileHelper.OpenFileOutputStream(filePath, true);
                si.CopyTo(os);                
            }
            catch (Exception e1)
            {
                Log.Error(Common.CommonFunctions.APP_NAME, e1.Message);
                return null;
            }
            finally
            {
                if (si != null)
                {
                    si.Close();

                }
                if (os != null)
                {
                    os.Close();
                }
            }

            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName); ;
        }



       internal static Stream OpenFileOutputStream(string filePath, bool internalAppSpace = true, long lengthInBytes = 0, string mimetype = null, bool append= false)
        {
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filePath);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            { 
                if (internalAppSpace)
                {
                    filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
                }
                else
                {
                    return MediaStoreHelper.OpenFileOutputStream(fileName, lengthInBytes, mimetype, append ? "wa" : string.Empty);

                }
            }
            else
            {
                filePath = System.IO.Path.Combine(Common.Settings.DataFolder, fileName);               
            }

            return File.Open(filePath,  append ? FileMode.Append : FileMode.Create);
        }

        internal static long GetLengthOfFile(string filePath, bool internalAppSpace = true)
        {
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filePath);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (internalAppSpace)
                {
                    filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
                }
                else
                {
                    return MediaStoreHelper.GetFileSize(filePath);
                }
            }
            else
            {
                filePath = System.IO.Path.Combine(Common.Settings.DataFolder, fileName);
            }
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

        static internal Stream CreateFile(Activity act, string filename, bool blExternal, string mimeType = null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (mimeType == null)
                {
                    // can only write binary data to app space  
                    System.IO.FileStream os = null;
                    var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var filePath = Path.Combine(documentsPath, filename);
                    os = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    return os;
                }
                else
                {
                    return MediaStoreHelper.OpenFileOutputStream(filename, -1, mimeType);
                }
            }
            else
            {
                return CreateFileLegacy(act, filename, blExternal);   
            }
        }

        static private FileStream CreateFileLegacy(Activity act, string filename, bool blExternal)
        {
            // this func can be deleted once you stop supporting android < 29 api
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
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                return MediaStoreHelper.GetFileText(act, filename, mediaStoreExtension);
            }
            else
            {
                return LoadTextLegacy(act, filename, blExternal);
            }

        }

        static internal bool CheckIfFileExists(string fileName, bool internalAppSpace = true, string mediaStoreFileExt = "")
        {
            string filePath = null;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (internalAppSpace)
                {
                    CommonFunctions comF = new CommonFunctions();
                    var fn = comF.GetFileNameFromPath(fileName);
                    var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    filePath = Path.Combine(documentsPath, fn);
                    
                }
                else
                {
                    return MediaStoreHelper.CheckIfFileExists(fileName, mediaStoreFileExt);
                }
            }
            else
            {
                // check if xml file exits
                var documentsPath = Settings.DataFolder;
                filePath = System.IO.Path.Combine(documentsPath, fileName);                         
            }

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