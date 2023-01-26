using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using System.Threading;
using Android.Media;

using Android.Util;
using System.Diagnostics;
using System.IO;

namespace TTtuner_2022_2.Audio
{

    internal interface IAudioPlayer
    {
        int Duration
        {
            get; 
        }
        bool IsPlaying { get; set; }

        int CurrentPosition
        {
            get; 
        }     
        
       // float Speed { get; }

        void SeekTo(int intPosition);

        void SetupPlayer( string strFilename, float flSpeed, bool blStartPlayAfterSetup, int intPositionToStartFrom  = 0, bool deleteFileOnExit = false);

        void Pause(bool stopPlay = true);

        void Play();

        void Stop();

        void Destroy();

        void ChangeSpeed(float flSpeed);
    }



}