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
using static System.Net.WebRequestMethods;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("TTtunerUnitTests")]

namespace TTtuner_2022_2.Common
{
   
    // [System.Reflection.Obfuscation(Exclude = false, Feature = "preset(minimum);-anti dump;+ctrl flow;-invalid metadata;-resources;+rename(mode=letters);")]
    public class PathUtil
    {
        /*
         * Gets the file path of the given Uri.
         */
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressLint("NewApi") public static String getPath(Context context, Uri uri) throws URISyntaxException
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        public static string getPath(Context context, global::Android.Net.Uri uri)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final boolean needToCheckUri = Build.VERSION.SDK_INT >= 19;
            bool needToCheckUri = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;
            string selection = null;
            string[] selectionArgs = null;
            // Uri is different in versions after KITKAT (Android 4.4), we need to
            // deal with different Uris.
            if (needToCheckUri && DocumentsContract.IsDocumentUri(context.ApplicationContext, uri))
            {
                if (isExternalStorageDocument(uri))
                {
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String docId = DocumentsContract.getDocumentId(uri);
                    string docId = DocumentsContract.GetDocumentId(uri);
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String[] split = docId.split(":");
                    string[] split = docId.Split(":", true);
                    return global::Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                }
                else if (isDownloadsDocument(uri))
                {
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String id = DocumentsContract.getDocumentId(uri);
                    string id = DocumentsContract.GetDocumentId(uri);
                    uri = ContentUris.WithAppendedId(global::Android.Net.Uri.Parse("content://downloads/public_downloads"), Convert.ToInt64(id));
                }
                else if (isMediaDocument(uri))
                {
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String docId = DocumentsContract.getDocumentId(uri);
                    string docId = DocumentsContract.GetDocumentId(uri);
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String[] split = docId.split(":");
                    string[] split = docId.Split(":", true);
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final String type = split[0];
                    string type = split[0];
                    if ("image".Equals(type))
                    {
                        uri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        uri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        uri = MediaStore.Audio.Media.ExternalContentUri;
                    }
                    selection = "_id=?";
                    selectionArgs = new string[] { split[1] };
                }
            }
            if ("content".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
            {
                string[] projection = new string[] { MediaStore.Audio.Media.InterfaceConsts.Data };
                ICursor cursor = null;
                try
                {
                    cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                    int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data);
                    if (cursor.MoveToFirst())
                    {
                        return cursor.GetString(column_index);
                    }
                }
                catch (Exception e1)
                {
                    string str = e1.Message;
                }
            }
            else if ("file".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
            {
                return uri.Path;
            }
            return null;
        }


        /// <param name="uri"> The Uri to check. </param>
        /// <returns> Whether the Uri authority is ExternalStorageProvider. </returns>
        public static bool isExternalStorageDocument(global::Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /// <param name="uri"> The Uri to check. </param>
        /// <returns> Whether the Uri authority is DownloadsProvider. </returns>
        public static bool isDownloadsDocument(global::Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /// <param name="uri"> The Uri to check. </param>
        /// <returns> Whether the Uri authority is MediaProvider. </returns>
        public static bool isMediaDocument(global::Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }
    }

    //-------------------------------------------------------------------------------------------
    //	Copyright © 2007 - 2017 Tangible Software Solutions Inc.
    //	This class can be used by anyone provided that the copyright notice remains intact.
    //
    //	This class is used to convert some aspects of the Java String class.
    //-------------------------------------------------------------------------------------------









    internal class CommonFunctions
    {
        internal const string APP_NAME = "TUNE_TRACK";
        internal const string TEXT_EXTENSION = ".txt";
        internal const string STAT_FILE_EXTENSION = ".stt";
        internal const string WAV_FILE_EXTENSION = ".wav";
        internal const string DCB_FILE_EXTENSION = ".dcb";



        internal string TruncateStringLeft(string str, int intLength)
        {
            int i;
            char[] chrReturn = new char[intLength];
            int intLengthDiff;
            if (str.Length < intLength)
            {
                return str;
            }

            intLengthDiff = str.Length - intLength;

            for (i = 0; i < intLength; i++)
            {
                chrReturn[i] = '.';
            }

            if (intLength > str.Length)
            {
                return str;
            }


            for (i = str.Length - 1; i > str.Length - intLength + 2; i--)
            {
                chrReturn[i - intLengthDiff] = str[i];

            }

            return new string(chrReturn);
        }

        internal void RenameFile( string orignalName, string newName)
        {
            Java.IO.File fl1 = new Java.IO.File(orignalName);
            Java.IO.File fl2 = new Java.IO.File(newName);

            fl1.RenameTo(fl2);
            fl1.Delete();

            fl1.Dispose();
            fl2.Dispose();
        }

        internal void CopyStringToFile(string text, Java.IO.File file)
        {
            FileWriter fr = null;
            BufferedWriter br = null;
            try
            {
                fr = new FileWriter(file);
                br = new BufferedWriter(fr);

                string[] strArr = text.Split('\n');

                for (int i = 0; i < strArr.Length; i++)
                {
                    br.Write(strArr[i]);
                    br.NewLine();
                }
            }
            catch (Java.IO.IOException e)
            {
                throw e;
            }
            finally
            {
                try
                {
                    br.Close();
                    fr.Close();
                }
                catch (Java.IO.IOException e)
                {
                    throw e;
                }
            }

        }


        internal string TruncateStringRight(string str, int intLength)
        {
            int i;
            char[] chrReturn = new char[intLength];
            int intLengthDiff;


            if (str.Length < intLength)
            {
                return str;
            }
            intLengthDiff = str.Length - intLength;
            for (i = 0; i < intLength; i++)
            {
                chrReturn[i] = '.';
            }

            if (intLength > str.Length)
            {
                return str;
            }


            for (i = 0; i < intLength - 2; i++)
            {
                if (i < str.Length)
                {
                    chrReturn[i] = str[i];
                }

            }

            return new string(chrReturn);
        }

        internal bool DoesDcbFileExistForThisFreqFile(string strTextFilePath)
        {
            string strDecibelFilepath = GetFilePathAndNameWtihoutExtension(strTextFilePath) + CommonFunctions.DCB_FILE_EXTENSION;
            return FileHelper.CheckIfFileExists(strDecibelFilepath) ? true : false;
        }

        internal bool DoesWavFileExistForThisFreqFile(string strTextFilePath)
        {
            string strWavFilepath = GetFilePathAndNameWtihoutExtension(strTextFilePath) + CommonFunctions.WAV_FILE_EXTENSION;
            return FileHelper.CheckIfFileExists(strWavFilepath, false) ? true : false;
        }

        internal string GetDcbFileNameForThisFreqFile(string strTextFilePath)
        {
            return GetFilePathAndNameWtihoutExtension(strTextFilePath) + CommonFunctions.DCB_FILE_EXTENSION;
        }


        internal string ConvertSecondsToClockTime(double dbSeconds)
        {
            return ((int)dbSeconds / 60).ToString("00") + " : " + ((int)dbSeconds % 60).ToString("00");
        }



        internal int GetIndexOfItemInstringArray(string[] stringArray, string strItemVAlue)
        {

            for (int i = 0; i < stringArray.Length; i++)
            {
                if (strItemVAlue == stringArray[i])
                {
                    return i;
                }
            }

            throw new Exception("Item " + strItemVAlue + " not found in string array resource ");
        }

        internal void CopyArrays<T>(T[] arrSource, ref T[] arrDest)
        {
            if (arrSource.Length != arrDest.Length)
            {
                throw new Exception("Arrays are not of equal size");
            }

            for (int i = 0; i < arrSource.Length; i++)
            {
                arrDest[i] = arrSource[i];
            }
        }

        internal string GetFileNameFromPath(string path)
        {
            int intIndex = path.LastIndexOf('/');

            return path.Substring(intIndex + 1, path.Length - intIndex - 1);
        }


        internal string GetFilePathAndNameWtihoutExtension(string path)
        {
            int intIndex = path.LastIndexOf('.');
            return path.Substring(0, intIndex);
        }

        internal string GetFileNameWtihoutExtension(string path)
        {
            return path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - path.LastIndexOf('/') - 1);
        }
        internal string GetFileNameExtension(string path)
        {
            int intIndex = path.LastIndexOf('.');
            return path.Substring(intIndex + 1, 3).ToUpper();
        }

        internal void SetupXmlSettingsFile(Activity act)
        {
            WriteXmlSetttingsFiles(act);
            // load all settings from xml fiile

            Common.Settings.LoadAllSettings();

            WriteCsvFile(act);

            Settings.LoadTuningSytems(act);

            Common.Settings.UpdateNotePitchMapStaticClass();

        }
        public string ReadXMLfileIfExits()
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");

            Java.IO.File fl1 = new Java.IO.File(filePath);

            if (fl1.Exists())
            {
                using (var reader = new StreamReader(filePath))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                return null;
            }
           
        }

        private void WriteXmlSetttingsFiles(Activity act  )
        {
            Type type = this.GetType();
            // check if xml file exits
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, "Settings.Xml");

            string fileContents = ReadXMLfileIfExits();

            using (Java.IO.File fl1 = new Java.IO.File(filePath))
            {

                if (fl1.Exists())
                {
                    // sanity check
                    if (Common.Settings.SanityCheckSettings())
                    {
                        return;
                    }
                    else
                    {
#if Release_LogOutput
                        Logger.Info(Common.CommonFunctions.APP_NAME, "In WriteXmlSetttingsFiles - sanity check failed  ");
#endif
                        //first try and salvage some settings
                        try
                        {
                            Common.Settings.LoadAllSettings();
                        }
                        catch (Exception e)
                        {
                            // if a setting is not present then an exception will be thrown
                            // should catch this and continue
                        }
                        // delete settings file to rewrite
                        bool success = fl1.Delete();
                        bool exisits = fl1.Exists();
                        fl1.Dispose();
                        GC.Collect();

                        var list = Directory.GetFiles(documentsPath, "*");

                        // write new file
                        Settings.WriteXMLFileFromManifestToInternalStorage(act);
                        // see what settings are not present and load from the file
                        Settings.MergeLoadedSettingsWithFile();

                        fileContents = ReadXMLfileIfExits();
                    }
                }
                else
                {
                    // file doesn't exist
                    Settings.WriteXMLFileFromManifestToInternalStorage(act);
                }
            }
        }

        private void WriteCsvFile(Activity act)
        {
            // check if xml file exits
            var documentsPath = Settings.DataFolder;
            var fileName = Settings.TuningSystemsCsvFileName;


            if (FileHelper.CheckIfFileExists( fileName, false, CommonFunctions.TEXT_EXTENSION)) {
                return;
            }
           
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                string csvText = Settings.GetCsvFileTextFromManifest(act);
                MediaStoreHelper.WriteTextToFile(csvText, fileName);
            }
            else
            {
                // get the file from the manifest
                Settings.WriteCsvFileFromManifestToInternalStorage(act);
            }
        }

       
    }


    internal class StringResourceHelper
    {
        private static string[] m_arrScales = null;
        private static string[] m_arrTranspose = null;
        private static string[] m_arrSampleRate = null;
        private static string[] m_arrAudioPlayer = null;

        // for unit testing
        internal static void Init(string[] arrScales, string[] arrTranspose, string[] arrSampleRate, string[] arrAudioPlayer)
        {
            m_arrScales = (string[]) arrScales.Clone();
            m_arrTranspose = (string[]) arrTranspose.Clone();
            m_arrSampleRate = (string[]) arrSampleRate.Clone();
            m_arrAudioPlayer = (string[]) arrAudioPlayer.Clone();
        }

        internal static string[] ScalesArrayResource_Get(Context ctx)
        {
            if (m_arrScales == null)
            {
                m_arrScales = ctx.Resources.GetStringArray(Resource.Array.ScaleSelection_array);
            }

            return m_arrScales;
        }

        internal static string[] TransposeArrayResource_Get(Context ctx)
        {
            if (m_arrTranspose == null)
            {
                m_arrTranspose = ctx.Resources.GetStringArray(Resource.Array.TransposeSelection_array);

            }
            return m_arrTranspose;
        }
        
        internal static string[] SampleRateArrayResource_Get(Context ctx)
        {
            if (m_arrSampleRate == null)
            {
                m_arrSampleRate = ctx.Resources.GetStringArray(Resource.Array.SampleRate_array);

            }
            return m_arrSampleRate;
        }

        internal static string[] AudioPlayerArrayResource_Get(Context ctx)
        {
            if (m_arrAudioPlayer == null)
            {
                m_arrAudioPlayer = ctx.Resources.GetStringArray(Resource.Array.AudioPlayer_array);

            }
            return m_arrAudioPlayer;
        }


    }

    internal static class StringHelperClass
    {
        //----------------------------------------------------------------------------------
        //	This method replaces the Java String.substring method when 'start' is a
        //	method call or calculated value to ensure that 'start' is obtained just once.
        //----------------------------------------------------------------------------------
        internal static string SubstringSpecial(this string self, int start, int end)
        {
            return self.Substring(start, end - start);
        }

        //------------------------------------------------------------------------------------
        //	This method is used to replace calls to the 2-arg Java String.startsWith method.
        //------------------------------------------------------------------------------------
        internal static bool StartsWith(this string self, string prefix, int toffset)
        {
            return self.IndexOf(prefix, toffset, System.StringComparison.Ordinal) == toffset;
        }

        //------------------------------------------------------------------------------
        //	This method is used to replace most calls to the Java String.split method.
        //------------------------------------------------------------------------------
        internal static string[] Split(this string self, string regexDelimiter, bool trimTrailingEmptyStrings)
        {
            string[] splitArray = System.Text.RegularExpressions.Regex.Split(self, regexDelimiter);

            if (trimTrailingEmptyStrings)
            {
                if (splitArray.Length > 1)
                {
                    for (int i = splitArray.Length; i > 0; i--)
                    {
                        if (splitArray[i - 1].Length > 0)
                        {
                            if (i < splitArray.Length)
                                System.Array.Resize(ref splitArray, i);

                            break;
                        }
                    }
                }
            }

            return splitArray;
        }

        //-----------------------------------------------------------------------------
        //	These methods are used to replace calls to some Java String constructors.
        //-----------------------------------------------------------------------------
        internal static string NewString(sbyte[] bytes)
        {
            return NewString(bytes, 0, bytes.Length);
        }
        internal static string NewString(sbyte[] bytes, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString((byte[])(object)bytes, index, count);
        }
        internal static string NewString(sbyte[] bytes, string encoding)
        {
            return NewString(bytes, 0, bytes.Length, encoding);
        }
        internal static string NewString(sbyte[] bytes, int index, int count, string encoding)
        {
            return System.Text.Encoding.GetEncoding(encoding).GetString((byte[])(object)bytes, index, count);
        }

        //--------------------------------------------------------------------------------
        //	These methods are used to replace calls to the Java String.getBytes methods.
        //--------------------------------------------------------------------------------
        internal static sbyte[] GetBytes(this string self)
        {
            return GetSBytesForEncoding(System.Text.Encoding.UTF8, self);
        }
        internal static sbyte[] GetBytes(this string self, System.Text.Encoding encoding)
        {
            return GetSBytesForEncoding(encoding, self);
        }
        internal static sbyte[] GetBytes(this string self, string encoding)
        {
            return GetSBytesForEncoding(System.Text.Encoding.GetEncoding(encoding), self);
        }
        private static sbyte[] GetSBytesForEncoding(System.Text.Encoding encoding, string s)
        {
            sbyte[] sbytes = new sbyte[encoding.GetByteCount(s)];
            encoding.GetBytes(s, 0, s.Length, (byte[])(object)sbytes, 0);
            return sbytes;
        }
    }

    internal class NoteEventArgs : EventArgs
    {
        private string strNote;
        private double m_dblCentsCloseness;
        private double dblPitch;
        private double lngElapsedSeconds;
        private double dlLevel;


        internal NoteEventArgs(double strPitchValue, string strNoteValue, double dblCentsClosness, long lngElapsedMillisecValue, double dbL)
        {
            strNote = strNoteValue;
            m_dblCentsCloseness = dblCentsClosness;

            dblPitch = strPitchValue;

            lngElapsedSeconds = lngElapsedMillisecValue / 1000.0f;

            dlLevel = dbL;
        }

        internal string Note { get { return strNote; } }

        internal double Pitch { get { return dblPitch; } }
        internal double Time { get { return lngElapsedSeconds; } }

        // internal double PercentCloseness { get { return dblPrctCloseness; } }
        internal double CentsCloseness { get { return m_dblCentsCloseness; } }

        internal double DbLevel { get { return dlLevel; } }
    }




}