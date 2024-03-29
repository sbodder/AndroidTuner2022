﻿using Android.Media;
using Android.OS;
using Plugin.CurrentActivity;

namespace TTtuner_2022_2.Common
{
    /// <summary>
    /// This class write to both the standard logcat and also a file in the datafolder (for users who can't access logcat)
    /// </summary>
    class MediaMetaFacade
    {
        
        static public MediaMetadataRetriever GetRetriever(string strFilePath, bool internalAppSpace= true)
        {
            MediaMetadataRetriever mmr = new MediaMetadataRetriever();

            if ( (Build.VERSION.SdkInt >= BuildVersionCodes.R) && !internalAppSpace )
            {
                //assumes the file is in external storage and the file type is wave
                //mmr.SetDataSource(CrossCurrentActivity.Current.AppContext, MediaStoreHelper.GetFileUri(strFilePath, "", MediaStoreHelper.MIMETYPE_WAV)) ;
                mmr.SetDataSource(CrossCurrentActivity.Current.AppContext, StorageAccessFrameworkHelper.GetFileUri(strFilePath));

            }
            else
            {
                mmr.SetDataSource(strFilePath);
            }

            return mmr;


        }

    }

}
