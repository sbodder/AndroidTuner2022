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
using Android.Media;
using TTtuner_2022_2.EventHandlersTidy;

namespace TTtuner_2022_2.Audio
{
    internal class Media_AudioPlayer : IAudioPlayer

    {
        private MediaPlayer m_medPlayer = null;

        private string m_strWaveFileName;

        private int m_intCurrentTimePostion;

        private bool m_blStartPlayAfterSetup;

        public int Duration
        {
            get
            {
                return m_medPlayer.Duration;
            }
        }

        public bool IsReady { get; set; }

        public bool IsPlaying { get; set; }

        public int CurrentPosition
        {
            get
            {
                return m_medPlayer.CurrentPosition;
            }

        }


        public Media_AudioPlayer()
        {
            this.IsReady = false;
            this.IsPlaying = false;
        }

        private void MediaPlayerPrepared(Object source, EventArgs e)
        {

            m_medPlayer.SeekTo(m_intCurrentTimePostion);

            if (m_blStartPlayAfterSetup)
            {

                Play();

                // SeekTo(m_intCurrentMSPostion);
                m_blStartPlayAfterSetup = false;
            }

            this.IsReady = true;

        }

        public void Pause(bool stopPlay = true)
        {
            this.IsPlaying = false;
            if (stopPlay)
            {
                m_medPlayer.Pause();
            }
        }

        public void Play()
        {
            m_medPlayer.Start();

            this.IsPlaying = true;
        }

        public void Stop()
        {
            this.IsPlaying = false;

            if (m_medPlayer != null)
            {
                m_medPlayer.Stop();
            }
        }

        public void Destroy()
        {
            if (m_medPlayer != null)
            {
                m_medPlayer.Stop();
                m_medPlayer.Release();
                m_medPlayer.Dispose();
                m_medPlayer = null;
            }
           
        }

        public void SeekTo(int intPosition)
        {
            m_medPlayer.SeekTo(intPosition);
        }

        public void ChangeSpeed(float flSpeed)
        {
            bool blsPlaying = this.IsPlaying;
            Pause();
            SetupPlayer(m_strWaveFileName, flSpeed, blsPlaying, this.CurrentPosition);
        }

        public void SetupPlayer( string strFileName, float flPlayerSpeed, bool blStartPlayAfterSetup, int intPositionToStartFrom = 0)
        {
            m_strWaveFileName = strFileName;
            m_intCurrentTimePostion = intPositionToStartFrom;

            

            m_blStartPlayAfterSetup = blStartPlayAfterSetup;
            if (m_medPlayer != null)
            {
                cEventHelper.RemoveAllEventHandlers(m_medPlayer);
                Destroy();
            }

            m_medPlayer = new MediaPlayer();

            m_medPlayer.Prepared += MediaPlayerPrepared;

            try
            {

                m_medPlayer.SetDataSource(m_strWaveFileName);

                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
                {
                    PlaybackParams playbackParams = new PlaybackParams();
                    playbackParams.SetSpeed(flPlayerSpeed);
                   // playbackParams.SetSpeed(0.25f);
                 
                    m_medPlayer.PlaybackParams = playbackParams;
                }

                m_medPlayer.Prepare();
             //   m_medPlayer.SetAudioAttributes
            }
            catch (Exception e1)
            {
               
                throw new Exception(e1.Message);
            }
            
         
        }
    }
}