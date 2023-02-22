using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.App.AppSearch;
using Android.OS;
using AndroidX.DocumentFile.Provider;
using global::Android.Content;
using Plugin.CurrentActivity;
using Syncfusion.Data.Extensions;

namespace TTtuner_2022_2.Common
{
    class StorageAccessFrameworkHelper
    {
        static internal DocumentFile GetDocumentFileForDataFolder()
        {
            return DocumentFile.FromTreeUri(CrossCurrentActivity.Current.AppContext, Settings.DataStoreFolderUri);
        }

        static internal global::Android.Net.Uri GetFileUri(string filename)
        {
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filename);
            var dir = GetDocumentFileForDataFolder();
            var df = dir?.FindFile(fileName);

            return df == null ? null : df.Uri;
        }

        static internal Stream OpenFileInputStream(string filename)
        {
            var uri = GetFileUri(filename);             

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            return  uri == null ? null : resolver.OpenInputStream(uri); 
        }

        static internal void SaveTextFile(string filepath, string text)
        {
            var dir = StorageAccessFrameworkHelper.GetDocumentFileForDataFolder();
            CommonFunctions comF = new CommonFunctions();
            var fileName = comF.GetFileNameFromPath(filepath);
            var csvFile = dir.CreateFile("text/plain", fileName);

            //var os = OpenFileOutputStream(csvFile.Uri);


            using (var os = OpenFileOutputStream(csvFile.Uri))
            {
                using (StreamWriter writer = new StreamWriter(os))
                {
                    writer.Write(text);
                    writer.Close();
                    writer.DisposeAsync();
                }
            }
        }

        internal static Stream OpenFileOutputStream(string fileName, string mimetype, string openMode = "")
        {
            DocumentFile df;
            // check if the intent is to create a new file
            if (CheckIfFileExists(fileName) && openMode != "wa")
            {
                Java.IO.File fl = new Java.IO.File(GetNewFilePath(fileName));
                df = DocumentFile.FromFile(fl);
                return OpenFileOutputStream(df.Uri, openMode);
            }            

            df = CreateFile(fileName, mimetype);
            return OpenFileOutputStream(df.Uri, openMode);
        }

        internal static DocumentFile CreateFile(string fileName, string mimetype)
        {

            CommonFunctions cf = new CommonFunctions();
            string filename = cf.GetFileNameFromPath(fileName);

            var df = GetDocumentFileForDataFolder();
            return df.CreateFile(mimetype, filename );
        }

        internal static bool CheckIfFileExists(string fileName, string extension = "")
        {
            var filename = fileName + (string.IsNullOrEmpty(extension) ? "" : extension);
            var dir = GetDocumentFileForDataFolder();
            return dir?.FindFile(filename) != null;

        }

        internal static Stream OpenFileOutputStream(global::Android.Net.Uri uri, string openMode= "")
        {
            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            if (string.IsNullOrEmpty(openMode))
            {
                return resolver.OpenOutputStream(uri);
            }
            else
            {
                return resolver.OpenOutputStream(uri, openMode);
            }
        }

        internal static string GetNewFilePath(string filename)
        {
            return System.IO.Path.Combine(Settings.DataFolder, filename);

        }

        internal static IList<FileInfoItem> GetMediaFileListInDataDirectory()
        {
            CommonFunctions cf = new CommonFunctions();
            var df = GetDocumentFileForDataFolder();
            var dfARray = df.ListFiles();
            IList<FileInfoItem> list = new List<FileInfoItem>();

            foreach (DocumentFile d in dfARray)
            {
                var ext = cf.GetFileNameExtension(d.Name);
                if (ext == CommonFunctions.WAV_FILE_EXTENSION || ext == CommonFunctions.STAT_FILE_EXTENSION)
                {
                    list.Add(new FileInfoItem(d.Name, MediaStoreHelper.GetFilePathOfUri(d.Uri)));
                }
            }
            //var list = MediaStoreHelper.GetMediaFilesInAppDirectory();
            var list2 = FileHelper.GetStatsFileInInternalAppSpace();


            // this is the only directory that will be useful to retrieve if on scoped storage

            foreach (var item in list2)
            {
                list.Add(item);
            }

            return list.OrderBy(s => s.Name).ToList();
        }
    }
}