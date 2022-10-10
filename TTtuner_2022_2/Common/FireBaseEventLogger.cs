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
using Firebase.Analytics;
using System.Collections;

namespace TTtuner_2022_2.Common
{
    internal class FireBaseEventLogger
    {
        FirebaseAnalytics _firebaseAnalytics;
        public FireBaseEvents events;
        Hashtable _ht;
        string _deviceID;

        public FireBaseEventLogger(Activity act)
        {
            _firebaseAnalytics = FirebaseAnalytics.GetInstance(act);
            events = new FireBaseEvents();
            _ht = new Hashtable();
            var properties = events.GetType().GetProperties();
            int i = 1;

            foreach (var field in properties)
            {
                _ht.Add( field.GetValue(events), i.ToString());
                i++;
            }
            _deviceID = global::Android.Provider.Settings.Secure.GetString(global::Android.App.Application.Context.ContentResolver, global::Android.Provider.Settings.Secure.AndroidId);
        }

        public void SendEvent(string eventName, string desc)
        {
            var bundle = new Bundle();
            bundle.PutString(FirebaseAnalytics.Param.ItemId, _ht[eventName].ToString());
            bundle.PutString(FirebaseAnalytics.Param.ItemName, "itemName");
            bundle.PutString(FirebaseAnalytics.Param.Content, $"Device Id: {_deviceID}, {desc}");
            _firebaseAnalytics.LogEvent(eventName, bundle);
        }
    }

    internal class FireBaseEvents
    {
        public string AUDIO_INIT { get; }  = "audio_init";
        public string AUDIO_NOT_INIT { get; } = "audio_could_not_init";
        public string GAUGE_FRAG_CREATE_EXCEP { get; } = "gauge_frag_create_excep";
        public string AUDIO_READ_ERROR { get; } = "audio_read_error";
        public string PITCH_REC_FRAGS_NOT_SETUP { get; } = "pitch_rec_frags_not_setup";
        public string PITCH_REC_ADDED_DP { get; } = "pitch_rec_added_dp";
        public string PITCH_REC_NO_NOTE { get; } = "pitch_rec_no_note";
    }
}