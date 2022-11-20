using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using Google.Android.Material.Snackbar;
using Environment = Android.OS.Environment;
using TTtuner_2022_2.Plot;

namespace TTtuner_2022_2
{
    [Activity(Label = "TTtuner", MainLauncher = true)]
    public class SplashScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {

                checkAndRequestPermissionsForAndroid11Plus();
            }
            else
            {
                startMainActivity();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            string strMess;
            // m_prgBAr.Visibility = ViewStates.Gone;

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {

                checkAndRequestPermissionsForAndroid11Plus();
            }
            else
            {
                startMainActivity();
            }


        }

        private void checkAndRequestPermissionsForAndroid11Plus()
        {
            if (!Environment.IsExternalStorageManager)
            {

                Snackbar.Make(FindViewById(global::Android.Resource.Id.Content), "Permission needed! Allow TTtuner access the file system in settings", Snackbar.LengthIndefinite)
                .SetAction("Settings", new Android12PlusPermissionClickHandler(this)).Show();
            }
            else
            {
                startMainActivity();
            }
        }

        private void startMainActivity() 
        {
            var mainActivity = new Intent(this, typeof(MainActivity));
            StartActivity(mainActivity);
        }
    }

}