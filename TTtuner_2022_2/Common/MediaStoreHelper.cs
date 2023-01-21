using Android.App;
using Android.Content;
using Android.Database;
using Android.Widget;
using BE.Tarsos.Dsp.Mfcc;
using Javax.Security.Auth;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.IO;

namespace TTtuner_2022_2.Common
{
    internal static class MediaStoreHelper
    {
        static internal bool CheckIfFileExists(Activity act, string filename, string mediaStoreExtension)
        {
            var uri = GetFileUri(act, filename, mediaStoreExtension);

            return uri == null? false : true;
        }

        static private global::Android.Net.Uri GetFileUri(Activity act, string filename, string mediaStoreExtension = "")
        {
            // READ FILE from folder in Downloads
            var documentsPath = Settings.MediaStoreFolder;
            CommonFunctions comFunc = new CommonFunctions();
            var newfilename = filename + mediaStoreExtension;
            ICursor cursor = null;

            try
            {
                var projection = new List<string>()
                    {
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.Id,
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName,
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DateAdded,
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.Title,
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.RelativePath,
                        global::Android.Provider.MediaStore.Downloads.InterfaceConsts.MimeType,
                    }.ToArray();

                string selection = global::Android.Provider.MediaStore.Downloads.InterfaceConsts.MimeType + " = ? AND " + global::Android.Provider.MediaStore.Downloads.InterfaceConsts.RelativePath + " = ? AND " +
                  global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName + " = ? ";

                var selectionArgs = new List<string>()
                    {
                        "text/plain", documentsPath, newfilename
                    };


                ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
                cursor = contentResolver.Query(global::Android.Provider.MediaStore.Downloads.ExternalContentUri, projection, selection, selectionArgs.ToArray(), null);

                if (cursor != null && cursor.Count > 0)
                {
                    int idColumn = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.Id);
                    int dispNameColumn = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName);
                    int relativePathCol = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.RelativePath);

                    cursor.MoveToFirst();

                    do
                    {
                        long id = cursor.GetLong(idColumn);
                        string displayName = cursor.GetString(dispNameColumn);
                        string relativePath = cursor.GetString(relativePathCol);

                        if (displayName.Equals(newfilename))
                        {
                            global::Android.Net.Uri uri1 = global::Android.Provider.MediaStore.Downloads.ExternalContentUri.BuildUpon().AppendPath(id.ToString()).Build();

                            return uri1;
                        }
                    }
                    while (cursor.MoveToNext());
                }
            }
            catch (Exception e1)
            {
                Toast.MakeText(act, "Cannot find file " + newfilename + " : " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();

            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                    cursor.Dispose();
                }
            }

            return null;

        }

        static internal string GetFileText(Activity act, string filename, string mediaStoreExtension)
        {
            var uri = GetFileUri(act, filename, mediaStoreExtension);

            if (uri == null)
            {
                return null;
            }

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            using (System.IO.Stream stream = resolver.OpenInputStream(uri))
            {
                // Perform operations on "stream".
                byte[] bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, bytesInStream.Length);
                var str = System.Text.Encoding.Default.GetString(bytesInStream);
                return str;
            }
        }

        static internal Stream OpenFileInputStream( string filename, string mediaStoreExtension)
        {
            var uri = GetFileUri(CrossCurrentActivity.Current.Activity, filename, mediaStoreExtension);

            if (uri == null)
            {
                return null;
            }

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

           

            return resolver.OpenInputStream(uri);           
        }

        static internal Stream OpenFileOutputStream(string filename, long lengthInBytes)
        {
            var uri = CreateFileUri(filename, lengthInBytes);

            if (uri == null)
            {
                return null;
            }

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            return resolver.OpenOutputStream(uri);
        }

        static internal global::Android.Net.Uri CreateFileUri(string filename, long lengthInByles)
        {
            string fileNameWithoutExt = Path.ChangeExtension(filename, null);

            int fileSize = (int)lengthInByles;

            ContentValues values = new ContentValues();
            ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            var relativeLocation = Settings.MediaStoreFolder;


            values.Put(global::Android.Provider.MediaStore.IMediaColumns.Title, filename);
            values.Put(global::Android.Provider.MediaStore.IMediaColumns.MimeType, "text/plain");
            values.Put(global::Android.Provider.MediaStore.IMediaColumns.Size, fileSize);
            values.Put(global::Android.Provider.MediaStore.IMediaColumns.RelativePath, relativeLocation);
            values.Put(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName, filename);

            global::Android.Net.Uri newUri;
            

            try
            {
                newUri = contentResolver.Insert(global::Android.Provider.MediaStore.Downloads.ExternalContentUri, values);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to get data back from content resolver. Filename: " + filename);
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                System.Diagnostics.Debug.Flush();
                return null;
            }

            return newUri;

        }

        static internal bool WriteFile(string contents, string filename)
        {
            System.IO.Stream saveStream;
            ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            var uri = CreateFileUri(filename, contents.Length);

            if (uri == null)
            {
                return false;
            }


            try
            {
                saveStream = contentResolver.OpenOutputStream(uri);

                using (StreamWriter writer = new StreamWriter(saveStream))
                {
                    writer.Write(contents);
                    writer.Close();
                    writer.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed file write: " + filename);
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                System.Diagnostics.Debug.Flush();
                return false;
            }

            return true;
        }
    }
}