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

namespace TTtuner_2022_2.Services
{
    // the raison d'etre of this service is so to intercept the event of closing the app
    // by swiping using the android recent app list. This way I can flush all temp settings
    // to disk in the settings class

    //[Service]
    //[IntentFilter(new String[] { "com.xamarin.TTtunerService" })]
    //class TTtunerService : Service
    //{
    //    public override IBinder OnBind(Intent intent)
    //    {
    //        return null;
    //    }

    //    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    //    {
    //        return StartCommandResult.NotSticky;
    //    }

    //    public override void OnTaskRemoved(Intent rootIntent)
    //    {
    //        Common.Settings.FlushAllUnsavedSettingsToDisk();
    //        //Code here
    //        StopSelf();
    //    }
    //}
}