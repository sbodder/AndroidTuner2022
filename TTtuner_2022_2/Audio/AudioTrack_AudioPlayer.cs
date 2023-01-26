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
using Java.IO;
using System.Threading.Tasks;
using Android.Util;
using System.Threading;
using System.Reflection;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Audio
{
    internal class AudioTrack_AudioPlayer : IAudioPlayer
    {
        // this should be the same as the recorder sameplerate

        private AudioTrack m_audTrack = null;
        System.IO.Stream m_fsInput = null;
        private byte[] m_rawData = null;
        const int WAVE_HEADER_SIZE_BYTES = 44;
        private int m_BufferSize;
        const int BYTES_PER_ELEMENT = 2; // 2 bytes in 16bit format
        long m_numChannels;
        private int m_intLastPlayerHeadPoistionWhenFileWasRead = 0;
        private string m_strWaveFileName;
        private Object thisLock = new Object();
        private bool m_blStartPlayAfterSetup;

        private int m_intDuration;
        private bool m_blEndPlayback = false;
        private int m_intSampleRate;
        private float m_flSpeed = 1f;
        private bool _deleteFileOnExit;

        private long BytesPerSecondInAudioFile
        {
            get
            {
                return m_intSampleRate * m_numChannels * BYTES_PER_ELEMENT;
            }
        }

        public int Duration
        {
            get
            {
                return m_intDuration;
            }
        }


        public float Speed
        {
            get
            {
                return m_flSpeed;
            }

        }


        public bool IsPlaying { get; set; }

        public int CurrentPosition
        {
            get
            {
                int intReturnVal;
                int inTimeLagAdjustment;

                // Logger.Info(Common.CommonFunctions.APP_NAME, "In get CureentPosition, file position is " + m_input.Position);

                // Logger.Info(Common.CommonFunctions.APP_NAME, "In get CureentPosition, track Head position is " + m_audTrack.PlaybackHeadPosition);

                // intReturnVal = (int)Math.Floor(((m_input.Position + (double)(m_audTrack.PlaybackHeadPosition % READ_BUFFER_SIZE)) / BytesPerSecondInAudioFile) * 1000);
                //return (int) Math.Floor( ( (double) m_audTrack.PlaybackHeadPosition / m_audTrack.SampleRate) * 1000.0 );

                intReturnVal = (int)Math.Floor(((m_fsInput.Position) / (double)BytesPerSecondInAudioFile) * 1000) + ConvertFramesToTime((m_audTrack.PlaybackHeadPosition - m_intLastPlayerHeadPoistionWhenFileWasRead));

                // Logger.Info(Common.CommonFunctions.APP_NAME, "In get CureentPosition, return value (time) is " + intReturnVal);


                // graph seems to lag behind audio at lower play speeds - adjust for this
                inTimeLagAdjustment = (int)((1f / m_flSpeed) * 25);

                return intReturnVal - inTimeLagAdjustment;

                // return (int)Math.Floor(((m_input.Position  - (float)WAVE_HEADER_SIZE_BYTES) / BytesPerSecondInAudioFile) * 1000);
            }
        }

        public AudioTrack_AudioPlayer()
        {
            this.IsPlaying = false;
        }

        public void Pause(bool stopPlay = true)
        {
            this.IsPlaying = false;
            if (stopPlay)
            {
                m_audTrack.Pause();
            }
        }

        public void Play()
        {
            m_audTrack.Play();
            //m_audTrack.Write(m_rawData, 0, m_rawData.Length);

            this.IsPlaying = true;
        }

        public void Stop()
        {
            this.IsPlaying = false;
            if (m_audTrack != null)
            {
                m_audTrack.Stop();
            }
        }

        public void Destroy()
        {
            if (m_audTrack != null)
            {
                m_audTrack.Stop();
                m_audTrack.Dispose();
                m_audTrack = null;
            }

            if (m_fsInput != null)
            {
                m_fsInput.Close();
                m_fsInput.Dispose();
            }

            m_blEndPlayback = true;

            if (_deleteFileOnExit)
            {
                FileHelper.DeleteFile(m_strWaveFileName);
            }

        }

        public void ChangeSpeed(float flSpeed)
        {
            float flVolume;
            int retVal;

            lock (thisLock)
            {

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in audioPlayer: before Change speed, speed is " + flSpeed);
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in audioPlayer: setting playback rate to " + (int)Math.Floor(m_intSampleRate * flSpeed));
#endif
                m_audTrack.Pause();
                m_flSpeed = flSpeed;
                retVal = m_audTrack.SetPlaybackRate((int)Math.Floor(m_intSampleRate * flSpeed));

             

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in audioPlayer: Playback rate is now " + m_audTrack.PlaybackRate);
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in audioPlayer: the return val is  " + retVal);
#endif

                flVolume = (1 / flSpeed) * 4;
                // volume seems to lower at lower speeds so adjust accordingly
                m_audTrack.SetVolume(flVolume);


#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In ChangeSpeed in audioPlayer: after Change speed");
#endif
            }


        }

        private int ConvertFramesToTime(int intFrames)
        {
            return (int)Math.Floor(((double)intFrames / m_audTrack.SampleRate) * 1000.0);
        }

        private int ConvertTimeToFrames(int intPositionMs)
        {
            return (int)Math.Floor((intPositionMs * m_audTrack.SampleRate) / 1000.0);
        }

        public void SeekTo(int intPositionMs)
        {
            SeekToInternal(intPositionMs);
        }

        private void SeekToInternal(int intPositionMs, bool blAudioTrackIsInitialised = true)
        {
            //m_audTrack.SetPlaybackHeadPosition(ConvertTimeToFrames(intPosition));
            long intSeekPositionBytes = (long)Math.Floor(((intPositionMs / 1000f) * BytesPerSecondInAudioFile));
            // must be an even number as there are 2 bytes per sample
            if ((intSeekPositionBytes % 2) == 1)
            {
                intSeekPositionBytes++;
            }

            bool blWasPlaying = this.IsPlaying;


            if (this.IsPlaying)
            {
                Pause(false);
            }

            try
            {
                //  Logger.Info(Common.CommonFunctions.APP_NAME, "In Seek, before flush track head poition is  " + m_audTrack.PlaybackHeadPosition);
            }
            catch (Exception e1)
            {
                string strMsg = e1.Message;
            }
            // reset current buffer

            lock (thisLock)
            {
#if Release_LogOutput || DEBUG
                Logger.Info(Common.CommonFunctions.APP_NAME, "In SeekToInternal in crit sction ");
                Logger.Info(Common.CommonFunctions.APP_NAME, "In SeekToInternal in audio player: intPositionMs " + intPositionMs);
#endif
                m_audTrack.Pause();

                m_intLastPlayerHeadPoistionWhenFileWasRead = 0;

                if (blAudioTrackIsInitialised)
                {
                    m_audTrack.Flush();
                    // m_audTrack.Stop();
                    m_rawData = m_rawData.Select(c => { c = 0; return c; }).ToArray();

                    // Thread.Sleep(100);
                    //m_intLastPlayerHeadPoistionWhenFileWasRead = m_audTrack.PlaybackHeadPosition;
                }


                // Logger.Info(Common.CommonFunctions.APP_NAME, "In Seek, after flush track head poition is  " + m_audTrack.PlaybackHeadPosition);

                // seek to position in file

                m_fsInput.Seek(intSeekPositionBytes + WAVE_HEADER_SIZE_BYTES, System.IO.SeekOrigin.Begin);

                if (blWasPlaying)
                {
                    Play();
                }

#if Release_LogOutput || DEBUG
                Logger.Info(Common.CommonFunctions.APP_NAME, "In SeekToInternal Exiting crit sction... ");
#endif
            }



        }

        private void PlayThread()
        {
            // can think of frames as equivalent to sampples
            // 2 bytes per sample
            int retWrite;


            while (!m_blEndPlayback)
            {
                if (this.IsPlaying)
                {
                    lock (thisLock)
                    {


                        // Logger.Info(Common.CommonFunctions.APP_NAME, "Before read of file , file position is " + m_input.Position);
                        if (m_fsInput.Read(m_rawData, 0, m_BufferSize) != 0)
                        {

                            m_intLastPlayerHeadPoistionWhenFileWasRead = m_audTrack.PlaybackHeadPosition;
                            //   Logger.Info(Common.CommonFunctions.APP_NAME, "After read of file , file position is " + m_input.Position);
#if Release_LogOutput || DEBUG
                          //  Logger.Info(Common.CommonFunctions.APP_NAME, "In PlayThread in crit sction before write. First byte is  " + m_rawData[0] + " second bytel is " + m_rawData[1]);
#endif
                            //  Logger.Info(Common.CommonFunctions.APP_NAME, "Before write Data to track, track Head position is " + m_audTrack.PlaybackHeadPosition);
                            retWrite = m_audTrack.Write(m_rawData, 0, m_rawData.Length);
#if Release_LogOutput || DEBUG
                          //  Logger.Info(Common.CommonFunctions.APP_NAME, "In PlayThread in crit sction after write. RetWrite is " + retWrite);
#endif
                            //  Logger.Info(Common.CommonFunctions.APP_NAME, "after write Data to track, track Head position is " + m_audTrack.PlaybackHeadPosition);
                        }
                    }
                    //}
                }
            }
        }

        public void SetupPlayer(string strFileName, float flPlayerSpeed, bool blStartPlayAfterSetup, int intPositionToStartFrom = 0, bool deleteFileOnExit = false)
        {        
            ChannelOut chNum = ChannelOut.Mono;
            m_strWaveFileName = strFileName;
            _deleteFileOnExit = deleteFileOnExit;

            MediaMetadataRetriever mmr = MediaMetaFacade.GetRetriever(m_strWaveFileName);

            String durationStr = mmr.ExtractMetadata(MetadataKey.Duration);
            m_intDuration = Convert.ToInt32(durationStr);
            byte[] arrHeader = new byte[WAVE_HEADER_SIZE_BYTES];

            short shNumChannels;

            mmr.Release();
            mmr = null;

            m_blEndPlayback = true;
            // sleep so that the playthread has time to finish if it exists.
            Thread.Sleep(100);


            if (m_fsInput != null)
            {
                m_fsInput.Close();
                m_fsInput.Dispose();
            }

            try
            {                
                m_fsInput = FileHelper.OpenFileInputStream(strFileName, false, MediaStoreHelper.MIMETYPE_WAV);
                m_fsInput.Read(arrHeader, 0, (int)WAVE_HEADER_SIZE_BYTES);
            }
            catch (Exception e1)
            {
                throw new Exception(e1.Message);
            }

            // android is little endian
            m_intSampleRate = arrHeader[24] + (arrHeader[25] * 256) + (arrHeader[26] * 256 * 256) + (arrHeader[27] * 256 * 256 * 256);

            shNumChannels = BitConverter.ToInt16(arrHeader, 22);
            m_numChannels = (long)shNumChannels;
            if (shNumChannels == 1)
            {
                chNum = ChannelOut.Mono;
            }
            else if (shNumChannels == 2)
            {
                chNum = ChannelOut.Stereo;
            }

            if (m_audTrack != null)
            {
                m_audTrack.Flush();
                m_audTrack.Stop();
                m_audTrack.Release();
                m_audTrack.Dispose();
            }

            m_BufferSize = AudioTrack.GetMinBufferSize(m_intSampleRate, chNum, global::Android.Media.Encoding.Pcm16bit);
           // m_BufferSize = m_BufferSize * 2;
            m_rawData = new byte[(int)m_BufferSize];

            m_audTrack = new AudioTrack(Stream.Music, m_intSampleRate, chNum, global::Android.Media.Encoding.Pcm16bit, m_BufferSize, AudioTrackMode.Stream);

            m_audTrack.SetPlaybackRate((int)Math.Floor(m_intSampleRate * flPlayerSpeed));

            m_blEndPlayback = false;


            Task.Run(() =>
            {
                PlayThread();
            });

            SeekToInternal(intPositionToStartFrom, false);

            if (blStartPlayAfterSetup)
            {
                Play();
            }

        }
    }
}