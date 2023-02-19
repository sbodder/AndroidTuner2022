using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Systems;
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
        public const string MIMETYPE_WAV = "audio/wav";
        static internal bool CheckIfFileExists(string filename, string mediaStoreExtension = "")
        {

            var uri = CreateFileUri(filename + mediaStoreExtension, -1);

            if (uri == null)
            {
                return false;
            }


            string newFilename = GetFilePathOfUri(uri);

            return newFilename != filename + mediaStoreExtension;
        }

        static internal long GetFileSize(string fileName, string mediaStoreExtension = "", string mimeType = null)
        {
            // READ FILE from folder in Downloads
            var documentsPath = Settings.MediaStoreFolder;
            CommonFunctions comFunc = new CommonFunctions();
            var fn = comFunc.GetFileNameFromPath(fileName);
            var newfilename = fn + mediaStoreExtension;
            ICursor cursor = null;

            try
            {
                var uri = GetFileUri(fileName);
                AssetFileDescriptor fileDescriptor = CrossCurrentActivity.Current.AppContext.ContentResolver.OpenAssetFileDescriptor(uri, "r");
                long fileSize = fileDescriptor.Length;

                return fileSize;
             
            }
            catch (Exception e1)
            {
                Toast.MakeText(CrossCurrentActivity.Current.Activity, "Cannot find file " + newfilename + " : " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();

            }

            return -1;


        }

        static internal global::Android.Net.Uri GetFileUri(string fileName, string mediaStoreExtension = "", string mimeType = null)
        {
            // READ FILE from folder in Downloads
            var documentsPath = Settings.MediaStoreFolder;
            CommonFunctions comFunc = new CommonFunctions();
            var fn = comFunc.GetFileNameFromPath(fileName);
            var newfilename = fn + mediaStoreExtension;
            ICursor cursor = null;

            try
            {
                string selection = 
                    //global::Android.Provider.MediaStore.Downloads.InterfaceConsts.MimeType + " = ? AND " +
                    MediaStore.Downloads.InterfaceConsts.RelativePath + " = ? AND " +
                  MediaStore.Downloads.InterfaceConsts.DisplayName + " = ? ";
               

                var selectionArgs = new List<string>()
                    {
                        //mimeType == null? "text/plain" : mimeType,
                    documentsPath, newfilename
                    };


                ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
                cursor = contentResolver.Query(global::Android.Provider.MediaStore.Downloads.ExternalContentUri, GetProjectionList(), selection, selectionArgs.ToArray(), null);

                if (cursor != null && cursor.Count > 0)
                {
                    var (idColumn, dispNameColumn, relativePathCol) = GetIdNameAndPathCols(cursor);

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
                Toast.MakeText(CrossCurrentActivity.Current.Activity, "Cannot find file " + newfilename + " : " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();

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


        static internal IList<FileInfoItem> GetMediaFilesInAppDirectory()
        {
            // READ FILE from folder in Downloads
            var documentsPath = Settings.MediaStoreFolder;
            CommonFunctions comFunc = new CommonFunctions();
            ICursor cursor = null;

            IList<FileInfoItem> fiArray = new List<FileInfoItem>();

            try
            {
                string selection =
                    //global::Android.Provider.MediaStore.Downloads.InterfaceConsts.MimeType + " = ? AND " +
                    MediaStore.Downloads.InterfaceConsts.RelativePath + " = ? AND " +
                  MediaStore.Downloads.InterfaceConsts.DisplayName + " like ? ";


                var selectionArgs = new List<string>()
                    {
                        //mimeType == null? "text/plain" : mimeType,
                    documentsPath, "%.WAV"
                    };


                ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
                cursor = contentResolver.Query(global::Android.Provider.MediaStore.Downloads.ExternalContentUri, GetProjectionList(), selection, selectionArgs.ToArray(), null);

                if (cursor != null && cursor.Count > 0)
                {
                    var (idColumn, dispNameColumn, relativePathCol) = GetIdNameAndPathCols(cursor);

                    cursor.MoveToFirst();

                    do
                    {
                        long id = cursor.GetLong(idColumn);
                        string displayName = cursor.GetString(dispNameColumn);
                        string relativePath = cursor.GetString(relativePathCol);

                        fiArray.Add(new FileInfoItem(displayName, System.IO.Path.Combine(relativePath, displayName) ));
                    }
                    while (cursor.MoveToNext());
                }
            }
            catch (Exception e1)
            {
                Toast.MakeText(CrossCurrentActivity.Current.Activity, "Cannot find files " + comFunc.TruncateStringRight(e1.Message, 35), ToastLength.Long).Show();
                return null;
            }
            return fiArray;
        }

        static private (int, int, int) GetIdNameAndPathCols(ICursor cursor)
        {
            int idColumn = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.Id);
            int dispNameColumn = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.DisplayName);
            int relativePathCol = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Downloads.InterfaceConsts.RelativePath);

            return (idColumn, dispNameColumn, relativePathCol);
        }

        static private string[] GetProjectionList()
        {
            return new List<string>()
                    {
                        MediaStore.Downloads.InterfaceConsts.Id,
                        MediaStore.Downloads.InterfaceConsts.DisplayName,
                        MediaStore.Downloads.InterfaceConsts.DateAdded,
                        MediaStore.Downloads.InterfaceConsts.Title,
                        MediaStore.Downloads.InterfaceConsts.RelativePath,
                        MediaStore.Downloads.InterfaceConsts.MimeType,
                    }.ToArray();
        }

        static internal string GetFileText(Activity act, string filename, string mediaStoreExtension)
        {
            var uri = GetFileUri(filename, mediaStoreExtension);

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

        static internal Stream OpenFileInputStream( string filepath, string mediaStoreExtension = "", string mimeType = null)
        {
            CommonFunctions comF = new CommonFunctions();
            var filename = comF.GetFileNameFromPath(filepath);
            var uri = GetFileUri(filename, mediaStoreExtension, mimeType);

            if (uri == null)
            {
                return null;
            }

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;     
            return resolver.OpenInputStream(uri);           
        }

        static internal bool DeleteFile(string filename, string mediaStoreExtension = "")
        {
            var uri = GetFileUri(filename, mediaStoreExtension);
            return DeleteUri(uri);
        }

        static private bool DeleteUri(global::Android.Net.Uri uri)
        {
            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            return resolver.Delete(uri, null) > 0;
        }

        static internal Stream OpenFileOutputStream(string filename, long lengthInBytes, string mimeType = null, string openMode = "" )
        {
            var uri = CreateFileUri(filename, lengthInBytes, mimeType);

            var urlFileName = GetFilePathOfUri(uri);

            if (urlFileName != filename)
            {

                Toast.MakeText(CrossCurrentActivity.Current.Activity, "File" + filename + "already exists in the folder, please move/remove and try again", ToastLength.Long).Show();
                return null;
            }

            if (uri == null)
            {
                return null;
            }

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            var os =  openMode == string.Empty ? resolver.OpenOutputStream(uri) : resolver.OpenOutputStream(uri, openMode);

            return os;
        }

        static public string GetFilePathOfUri(global::Android.Net.Uri uri)
        {
            String result = null;
            try
            {
                Java.IO.File file = new Java.IO.File(uri.LastPathSegment);//create path from uri
                String[] split = file.Path.Split(":");//split the path.
                result = split[1];//assign it to a string(your choice).
            }
            catch
            {
                return null;
            }
            return result;
        }



        static internal global::Android.Net.Uri CreateFileUri(string filename, long lengthInByles = -1, string mimeType=null)
        {
            string fileNameWithoutExt = System.IO.Path.ChangeExtension(filename, null);

            int fileSize = (int)lengthInByles;

            ContentValues values = new ContentValues();
            ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            var relativeLocation = Settings.MediaStoreFolder;


            values.Put(global::Android.Provider.MediaStore.IMediaColumns.Title, filename);
            values.Put(global::Android.Provider.MediaStore.IMediaColumns.MimeType, mimeType !=null ? mimeType : "text/plain");
            if (lengthInByles != -1)
            {
                values.Put(global::Android.Provider.MediaStore.IMediaColumns.Size, fileSize);
            }
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

        static internal string GetNewFilePath(string filename)
        {
            return System.IO.Path.Combine(Settings.MediaStoreFolder, filename);
        }

        static internal bool WriteTextToFile(string contents, string filename, string mimeType = null)
        {
            System.IO.Stream saveStream;
            ContentResolver contentResolver = CrossCurrentActivity.Current.AppContext.ContentResolver;

            if (string.IsNullOrEmpty(contents)) {
                return false;
            } 
            var uri = CreateFileUri(filename, contents.Length, mimeType);

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