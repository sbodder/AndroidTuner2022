using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms.Platform.Android;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace TTtuner_2022_2.Common
{
    internal class PermissionHelper
    {
        internal const int REQUEST_PERMISSIONS = 3; //arbitrary constant
        internal const int REQUEST_PERMISSIONS_API_30_AND_GREATER = 4;
        static internal void RequestPermissions( ref bool permissionsOk)
        {
            var act = CrossCurrentActivity.Current.Activity;
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                if (ArePermissionsGrantedAndroid11AndGreater())
                {
                    permissionsOk = true;
                }
                else
                {
                    AndroidX.Core.App.ActivityCompat.RequestPermissions(act,
                        new String[] { Manifest.Permission.RecordAudio, Manifest.Permission.Internet },
                        REQUEST_PERMISSIONS_API_30_AND_GREATER);
                }
            }
            else
            {
                if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.AppContext, Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted
               && AndroidX.Core.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.AppContext, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted
              )
                {
                    // We have permissions, go ahead and use the app
                    permissionsOk = true;
                }
                else
                {
                    permissionsOk = false;
                    //  permissions are not granted. If necessary display rationale & request.
                    AndroidX.Core.App.ActivityCompat.RequestPermissions(act,
                           new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.RecordAudio, Manifest.Permission.Internet },
                           REQUEST_PERMISSIONS);
                }
            }
        }

        static private bool ArePermissionsGrantedAndroid11AndGreater()
        {
            // list of all persisted permissions for our app           

            if (!(ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.AppContext, Manifest.Permission.Internet) == Permission.Granted)
           || !(ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.AppContext, Manifest.Permission.RecordAudio) == Permission.Granted)
                     )
            {
                return false;
            }

            return AreStoragePermissionsGrantedAndroid11AndGreater();
        }

        static internal bool AreStoragePermissionsGrantedAndroid11AndGreater()
        {

            ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
            var uriString = Settings.DataStoreFolderUriString;
            var list = resolver.PersistedUriPermissions;
            if (string.IsNullOrEmpty(uriString))
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var persistedUriString = item.Uri.ToString();
                if (persistedUriString == uriString && item.IsWritePermission && item.IsReadPermission)
                {
                    return true;
                }
            }

            return false;
        }

        static internal AndroidX.AppCompat.App.AlertDialog.Builder RequestStoragePermissionsAndroid11AndGreater()
        {

            //Toast.MakeText(this, "On the next Dialog please select the folder for TTtuner files", ToastLength.Long).Show();
            AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(CrossCurrentActivity.Current.Activity);
            alert.SetTitle("On the next dialog please select the data folder for TTtuner files");
            alert.SetPositiveButton("OK", (senderAlert, argus) =>
            {
                var intent = new Intent(Intent.ActionOpenDocumentTree);
                //intent.SetFlags(ActivityFlags.GrantPersistableUriPermission);

                intent.AddFlags(
                    ActivityFlags.GrantReadUriPermission |
                    ActivityFlags.GrantWriteUriPermission |
                    ActivityFlags.GrantPersistableUriPermission |
                    ActivityFlags.GrantPrefixUriPermission);

                CrossCurrentActivity.Current.Activity.StartActivityForResult(intent, REQUEST_PERMISSIONS_API_30_AND_GREATER);
            });

            return alert;
        }

        static internal void StorePermissionsResult(Intent data)
        {
            var sp = new Common.SharedPreferences();
            if (data != null)
            {
                //this is the uri user has provided us
                global::Android.Net.Uri treeUri = data != null ? data.Data : null;
                Settings.DataStoreFolderUriString = sp.StoreDataFolderUri(treeUri.ToString());
                Settings.DataFolder = MediaStoreHelper.GetFilePathOfUri(treeUri);

                // store permissions result

                ContentResolver resolver = CrossCurrentActivity.Current.AppContext.ContentResolver;
                var takeFlags = data.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                resolver.TakePersistableUriPermission(treeUri, takeFlags);
            }

        }

        static internal void ExitActivityIfAnyPermissionNotGranted(Permission[] grantResults)
        {
            foreach (Permission p1 in grantResults)
            {
                if (!(p1 == Permission.Granted))
                {
                    Toast.MakeText(CrossCurrentActivity.Current.Activity, "Application Exiting...", ToastLength.Long).Show();
                    CrossCurrentActivity.Current.Activity.FinishAffinity();
                }
            }
        }
    }
}