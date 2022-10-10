using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using global::Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using BE.Tarsos.Dsp;
using BE.Tarsos.Dsp.IO;
using BE.Tarsos.Dsp.IO.Android;
using BE.Tarsos.Dsp.Rubberband;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Audio
{
    public class RubberBand_AudioPlayer : IAudioPlayer
    {
        private System.IO.FileStream m_fsInput;
        private int m_intSampleRate;
        private long m_numChannels;
        const int WAVE_HEADER_SIZE_BYTES = 44;
        AudioDispatcherControllable m_ad = null;
        private TarsosDSPAudioFormat m_TarosFormat;
        AndroidAudioPlayer m_ap = null;
        double m_dbTimeRatio;
        private ChannelOut m_chNum;
        RubberBandAudioProcessor m_rbs = null;
        string m_strWaveFileName;
        int m_intDuration;
        double m_dbSecondsElasped = 0f;
        int m_bufferSize;

        public int Duration
        {
            get { return m_intDuration; }
        }
        public bool IsPlaying { get; set; }



        public int CurrentPosition
        {
            get
            {
                double dbTEmp;

                if (m_ad == null || m_ad.IsStopped)
                {
                    dbTEmp = m_dbSecondsElasped;
                }
                else
                {
                    dbTEmp = m_ap.SecondsProcessed + ((m_ap.PositionMs / 1000f) / m_dbTimeRatio);
                }
                // this is for the case where the audio player starts and has an empty buffer for the first few seconds
                // the current position will be 0 even though the time elapsed is a t > 0



#if Release_LogOutput
                    Logger.Info(Common.CommonFunctions.APP_NAME, "CurrentPosition  : dbTEmp " + dbTEmp);

                    Logger.Info(Common.CommonFunctions.APP_NAME, "\n\nm_ad.SecondsProcessed() : " + m_ad.SecondsProcessed());
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_dbTimeRatio : " + m_dbTimeRatio);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "LastPlaybackPostionBeforeWriteToTrack : " + m_ap.LastPlaybackPostionBeforeWriteToTrack);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_ap.PlaybackPostion : " + m_ap.PlaybackPostion);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_ap.SecondsProcessed : " + m_ap.SecondsProcessed);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_ap.DispatcherElapsedTime : " + m_ap.DispatcherElapsedTime);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_ap.PositionMs : " + (m_ap.PositionMs / 1000f));
                    Logger.Info(Common.CommonFunctions.APP_NAME, "m_ap.PositionMs / timeratio : " + (m_ap.PositionMs / 1000f) / m_dbTimeRatio);
                    Logger.Info(Common.CommonFunctions.APP_NAME, "CurrentPosition : " + (int)(dbTEmp * 1000f));
                    Logger.Info(Common.CommonFunctions.APP_NAME, "/////////////////////////////////////////////////////// \n\n ");
#endif
                    return (int)(dbTEmp * 1000f);
            }

             
         }
        


        public void SetupPlayer(string strFilename, float flSpeed, bool blStartPlayAfterSetup, int intPositionToStartFrom = 0)
        {
            MediaMetadataRetriever mmr = new MediaMetadataRetriever();
            m_dbTimeRatio = Math.Round(1 / flSpeed, 1);
            m_chNum = ChannelOut.Mono;
            m_strWaveFileName = strFilename;

            mmr.SetDataSource(m_strWaveFileName);

            String durationStr = mmr.ExtractMetadata(MetadataKey.Duration);
            m_intDuration = Convert.ToInt32(durationStr);
            byte[] arrHeader = new byte[WAVE_HEADER_SIZE_BYTES];

            short shNumChannels;
            mmr.Release();
            mmr.Dispose();
            mmr = null;

            if (m_fsInput != null)
            {
                m_fsInput.Close();
                m_fsInput.Dispose();
            }
            try
            {
                m_fsInput = new System.IO.FileStream(strFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                m_fsInput.Read(arrHeader, 0, (int)WAVE_HEADER_SIZE_BYTES);
            }
            catch (System.Exception e1)
            {
                throw new System.Exception(e1.Message);
            }
            m_intSampleRate = arrHeader[24] + (arrHeader[25] * 256) + (arrHeader[26] * 256 * 256) + (arrHeader[27] * 256 * 256 * 256);
            shNumChannels = BitConverter.ToInt16(arrHeader, 22);
            m_numChannels = (long)shNumChannels;

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "SetupPlayer : buffer size  is : " + m_bufferSize);
#endif
            SetupAudioProcessors(m_dbSecondsElasped, m_dbTimeRatio);

        }

        private void SetupAudioProcessors(double dbStartTime, double dbTimeRatio)
        {
            const int STREAM_MUSIC = 3;
            m_bufferSize = AudioTrack.GetMinBufferSize(m_intSampleRate, m_chNum, global::Android.Media.Encoding.Pcm16bit);
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : SetupAudioProcessors, starting ");
#endif
            if (m_ad == null)
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : SetupAudioProcessors, m_ad ");
#endif
                m_ad = AudioDispatcherFactory.FromPipeControllable(m_strWaveFileName, m_intSampleRate, m_bufferSize, 0);
                m_TarosFormat = m_ad.Format;

                if (dbStartTime > 0)
                {
                    m_ad.Skip(dbStartTime);
                }
            }

            if (m_rbs == null)
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : SetupAudioProcessors, m_rbs ");
#endif
                m_rbs = new RubberBandAudioProcessor(m_intSampleRate, dbTimeRatio, 1.0);
                m_ad.AddAudioProcessor(m_rbs);
            }

            if (m_ap == null)
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : SetupAudioProcessors, m_ap ");
#endif
                m_ap = new AndroidAudioPlayer(m_ad.Format, m_bufferSize, STREAM_MUSIC);
                m_ap.SetDispatcher(m_ad);
                m_ad.AddAudioProcessor(m_ap);
            }

            //  this.IsPlaying = false;

            m_ad.Pause();

            Task.Run(() =>
            {
                m_ad.Run();
            }).ContinueWith((encryptTask) => {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : ContinueWith, starting disposing ");
#endif
                m_dbSecondsElasped = m_ad.SecondsProcessed();

#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In rubberband player : ContinueWith, finished disposing ");
#endif

            });
        }



        public void Play()
        {
            if (m_ad != null)
            {
                m_ad.Play();
                this.IsPlaying = true;
            }
        }

         public void Stop()
        {
            if (m_ad != null)
            {
                m_ad.SeekTo((float)0.01f);
                m_ad.Pause();
            }

            this.IsPlaying = false;
        }

        public void Pause(bool stopPlay = true)
        {
            this.IsPlaying = false;
            if (m_ad != null)
            {
                m_ad.Pause();
            }
        }

        public void ChangeSpeed(float flSpeed)
        {
            if (m_ad != null)
            {
                m_dbTimeRatio = Math.Round(1 / flSpeed, 1);

                m_rbs.SetTimeRatio(m_dbTimeRatio);
            }
            
        }

        public void SeekTo(int intPosition)
        {
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "RubberBand_AudioPlayer SeekTo intPosition is : " + intPosition);
#endif
            m_dbSecondsElasped = intPosition / 1000f;

            if (m_ad != null)
            {
                m_ad.Pause();
                Thread.Sleep(100);
                m_ad.SeekTo( (float) m_dbSecondsElasped);
                m_ap.SecondsProcessed = (float) m_dbSecondsElasped;
            }

        }



        public void Destroy()
        {
            if (m_ad != null)
            {
                if (!m_ad.IsStopped)
                {
                    m_ad.Stop();
                } 
            }

            if (m_fsInput != null)
            {
                m_fsInput.Close();
                m_fsInput.Dispose();
            }

        }

    }
}