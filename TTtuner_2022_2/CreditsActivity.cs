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

namespace TTtuner_2022_2
{
    [Activity(Label = "CreditsActivity")]
    public class CreditsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            SetContentView(Resource.Layout.Credits);
            // Create your application here
        }
    }
}