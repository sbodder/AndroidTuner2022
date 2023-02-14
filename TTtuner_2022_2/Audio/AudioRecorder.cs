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
using TTtuner_2022_2.Common;
using TTtuner_2022_2.EventHandlersTidy;
using TTtuner_2022_2.DSP;
using Firebase.Analytics;
//using MediaFormat;

namespace TTtuner_2022_2.Audio
{
    internal class Audio_Record : Activity
    {
        FirebaseAnalytics _firebaseAnalytics;
        private const int RECORDER_SAMPLERATE = 8000;
        private const int READ_BLOCKING = 0;
        private const int READ_NON_BLOCKING = 1;
        private  int m_sampleRate;
        private AudioRecord m_recorder = null;
        // 2 bytes for pcm16bit
        private int BytesPerElement = 2;
        private bool sentError = false;
        private int _audioBufferSize = 0;

        private Thread m_recordingThread = null;
        private bool isRecording = false;
         
        internal int SAMPLES_IN_BUFF;
        DSP.MPM m_dspLib;
        internal bool IsRecording { get { return isRecording; } }
        long _lngTempTime;
        internal AudioRecord AudioRecorder
        {
            get
            {
                return m_recorder;
            }
        }

        Stopwatch m_stpWatch;
        long m_lngLastElapsedTime;
        long m_lngStartTime;

        internal event EventHandler<Common.NoteEventArgs> NewPitch;

        internal int SampleRate { get { return m_sampleRate; }   }

        private void ResetStartTime()
        {
            m_lngStartTime = SystemClock.UptimeMillis();
            _lngTempTime = 0;
        }

        internal Audio_Record()
        {
            
            int bufferSize = AudioRecord.GetMinBufferSize(RECORDER_SAMPLERATE, ChannelIn.Mono, Encoding.Pcm16bit);           
            m_stpWatch = new Stopwatch();
            m_lngLastElapsedTime = 0;
        }

        internal void Destroy()
        {
            cEventHelper.RemoveAllEventHandlers(this.NewPitch);
            StopRecording();
            Finish();
        }

        private static int GetMaxValidSampleRate()
        { 

                int maxRate = 0;
                foreach (int rate in new int[] { 8000, 11025, 16000, 22050, 44100, 48000 })
                {
                    int bufferSize = AudioRecord.GetMinBufferSize(rate, ChannelIn.Mono, Encoding.Pcm16bit);
                    if (bufferSize > 0)
                    {
                        maxRate = rate;
                    }
                }
                return maxRate;
        }


        internal void StartRecording(bool blSaveToFile, string strFileName = null)
        {
            // this is to make sure that the current recording thread 
            // has finished its clean up
            while (m_recorder != null)
            {
                Thread.Sleep(110);
            }
            isRecording = true;

            // m_sampleRate = GetMaxValidSampleRate();
            m_sampleRate =  Common.Settings.SampleRateInt;
            InitialiseAudioRecorder();

            SAMPLES_IN_BUFF = Common.Settings.NumberOfSamplesInBuffer;
            m_dspLib = new DSP.MPM(m_sampleRate, SAMPLES_IN_BUFF, 0.93f);

#if Release_LogOutput

             Logger.Info(Common.CommonFunctions.APP_NAME, "StartRecording number of samples is " + SAMPLES_IN_BUFF.ToString() + ".....");
#endif


            m_recordingThread = new System.Threading.Thread(() =>
            {
                GetPitchFromAudioData(blSaveToFile, strFileName);
            });
            m_recordingThread.Priority = System.Threading.ThreadPriority.AboveNormal;
            m_recordingThread.Start();
        }

        //convert short to byte
        private byte[] short2byte(short[] sData)
        {
            int shortArrsize = sData.Length;
            byte[] bytes = new byte[shortArrsize * 2];
            for (int i = 0; i < shortArrsize; i++)
            {
                bytes[i * 2] = unchecked((byte)(sData[i] & 0x00FF));
                bytes[(i * 2) + 1] = (byte)(sData[i] >> 8);
                //sData[i] = 0;
            }
            return bytes;

        }

       
        private void GetPitchFromAudioData(bool blSaveAudioDataToFile, string strFileName = null)
        {
            short[] sData = new short[SAMPLES_IN_BUFF];
            double? pitch = 0.0;
            double dblCentsCloseness = 0.0;
            double? dbLevel;
            string strNote;
            System.IO.Stream os = null;
            int intSamplingPeriodMs;
            Decibel dbLib = new Decibel();
            double flWindowTime = SAMPLES_IN_BUFF / (double)m_sampleRate;
            int intWindowTimeMs = (int)(flWindowTime * 1000);
            byte[] bData;
            int i = 0;
            //FireBaseEventLogger fb = new FireBaseEventLogger(this);

            if (blSaveAudioDataToFile)
            { 
                try
                {
                    os = FileHelper.CreateFile(this, strFileName, true);

                }
                catch (Java.IO.FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }
            //start the recorder
            VerifyAudioRecorderSetup();

            if (m_recorder.State == State.Initialized)
            {
                m_recorder.StartRecording();
                //fb.SendEvent(fb.events.AUDIO_INIT, $"Buffer size: {_audioBufferSize}");
           }
            else
            {
                // throw new Exception("Audio Recorder couuld not initialise");
                
            }

            m_lngStartTime = SystemClock.UptimeMillis();
            m_lngLastElapsedTime = m_lngStartTime;

            intSamplingPeriodMs = Common.Settings.PitchSmaplingPeriodMs;

            while (isRecording)
            {
                // gets the voice output from microphone to byte format
              
                int retRead = m_recorder.Read(sData, 0, sData.Length);
                if (retRead < 1 && !sentError)
                {
                    sentError = true;
                    //fb.SendEvent(fb.events.AUDIO_READ_ERROR, $"error: {retRead}");
                }
#if Release_LogOutput

               // Logger.Info(Common.CommonFunctions.APP_NAME, "GetPitchFromAudioData after reading from audio buffer....");
#endif
                _lngTempTime = SystemClock.UptimeMillis() - m_lngStartTime;
                
                if (blSaveAudioDataToFile)
                {
                    os.Write( short2byte(sData) , 0, SAMPLES_IN_BUFF * BytesPerElement);
                }

                try
                {
                    if ((SystemClock.UptimeMillis() - m_lngLastElapsedTime) > intSamplingPeriodMs)
                    {                        
                        m_lngLastElapsedTime = SystemClock.UptimeMillis();
                        pitch = null;

                       
                        dbLevel = dbLib.GetDbLevelFromShort(sData);
                                             
                        pitch = m_dspLib.getPitchFromShort(sData, dbLevel);                        

                        strNote = pitch == null? "" : Music.NotePitchMap.GetNoteFromPitch( (double) pitch, ref dblCentsCloseness);
#if DEBUG
                         Logger.Info(Common.CommonFunctions.APP_NAME, string.Format("note is {0} and pitch is {1:0.00} abd cents dev is {2:0.00}", strNote, pitch, dblCentsCloseness) );
#endif


                        RunOnUiThread(() =>
                        {
                            if (NewPitch != null)
                            {
                                if (strNote != null)
                                {
                                    long lngTime = Math.Max(0, _lngTempTime - intWindowTimeMs);
                                    Common.NoteEventArgs e = new Common.NoteEventArgs((double) (pitch == null? 0f : pitch), strNote, dblCentsCloseness, lngTime, (double) dbLevel);
#if DEBUG
                                    //Logger.Info(Common.CommonFunctions.APP_NAME, string.Format("time  is {0}, note is {1} and pitch is {2:0.00} and dBLevel is {2:0.00} ", lngTime, strNote, pitch, (double) dbLevel));
#endif
                                    NewPitch(this, e);
                                }

                            }
                        });
                    }


                }
                catch (Java.IO.IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            if (blSaveAudioDataToFile)
            {
                // close the file
                try
                {
                    os.Close();
#if Debug
                    Logger.Info(Common.CommonFunctions.APP_NAME, "Closing File....");
#endif
                }
                catch (Java.IO.IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            if (m_recorder.State == State.Initialized)
            {
                m_recorder.Stop();
            }
            m_recorder.Release();
            m_recorder = null;

#if Debug
            Logger.Info(Common.CommonFunctions.APP_NAME, "Recorder is null....");
#endif
        }

        private void VerifyAudioRecorderSetup()
        {
            int j = 0;

            while ((m_recorder.State == State.Uninitialized) && (j < 2))
            {
                int i = 0;
                while ((m_recorder.State == State.Uninitialized) && (i < 5))
                {
                    Thread.Sleep(100);
                    i++;
                }
                if (m_recorder.State == State.Uninitialized)
                {
                    m_recorder.Release();
                    InitialiseAudioRecorder(true);
                    j++;
                }
            }
        }

        private void InitialiseAudioRecorder(bool error=false)
        {
            _audioBufferSize = AudioRecord.GetMinBufferSize(m_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
            m_recorder = new AudioRecord(AudioSource.Mic, m_sampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _audioBufferSize * 10);
            //FireBaseEventLogger fb = new FireBaseEventLogger(this);

            if (error)
            {
                //fb.SendEvent(fb.events.AUDIO_NOT_INIT, $"Record state is {m_recorder.State.ToString()}," +
                //    $" buffer size is {_audioBufferSize}, sample Rate is {m_sampleRate}");                
            }
        }

        private bool isDataAllZeros(short[] sData)
        {
            bool bIsZero = true;
            for (int i = 0; i < sData.Length; i++)
            {
                if (sData[i] != 0)
                {
                    bIsZero = false;
                }
                    
            }

            return bIsZero;
        }

        internal void StopRecording()
        {
            ResetStartTime();
            // stops the recording activity
            if (null != m_recorder)
            {
                isRecording = false;
            }
        }



    }

}