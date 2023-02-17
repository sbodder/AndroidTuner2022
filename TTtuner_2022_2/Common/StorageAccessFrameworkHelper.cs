using System;
using System.IO;
using Android.App;
using Android.App.AppSearch;
using Android.OS;
using AndroidX.DocumentFile.Provider;
using global::Android.Content;
using Plugin.CurrentActivity;

namespace TTtuner_2022_2.Common
{
    class StorageAccessFrameworkHelper
    {
        static internal DocumentFile GetDocumentFileForDataFolder()
        {
            return DocumentFile.FromTreeUri(CrossCurrentActivity.Current.AppContext, Settings.DataStoreFolderUri);
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

        internal static Stream OpenFileOutputStream(global::Android.Net.Uri uri)
        {
            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            return resolver.OpenOutputStream(uri);
        }
    }
}