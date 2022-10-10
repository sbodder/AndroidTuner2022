﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using global::Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using BE.Tarsos.Dsp;
using BE.Tarsos.Dsp.IO.Android;
using BE.Tarsos.Dsp.Writer;
using Java.IO;

namespace TTtuner_2022_2.Audio
{
    internal class ConvertAudioFileToWave
    {
        AudioDispatcher m_ad = null;
        WriterProcessor m_wp = null;
        DetermineDurationProcessor m_ddp;
        double m_dbDurationInSeconds;
        float m_dbElapsedTime;
        private string m_strFileNameInput;
        private int m_intTargetSampleRate;
        private int m_intBufferSize;
        private string m_strWaveFileNameOutput;
        private bool m_blFinished;

        public float DurationInSeconds
        {
            get
            {
                return (float) m_dbDurationInSeconds;
            }
        }


        public float ElaspedTimeInSeconds
        {
            get
            {
                if (m_ad != null && !m_ad.IsStopped)
                {
                    m_dbElapsedTime = m_ad.SecondsProcessed();                  
                }
                return m_dbElapsedTime;
            }
        }

        public bool Finished
        {
            get
            {
                return m_blFinished;
            }
        }


        internal ConvertAudioFileToWave(int targetSampleRate, short nChannels, string strFileNameInput, string strWaveFileNameOutput)
        {    
            m_strFileNameInput = strFileNameInput;
            m_intTargetSampleRate = targetSampleRate;
            m_strWaveFileNameOutput = strWaveFileNameOutput;

            m_intBufferSize = AudioTrack.GetMinBufferSize(targetSampleRate, ChannelOut.Mono, global::Android.Media.Encoding.Pcm16bit);
            // input = new DataInputStream(new System.IO.FileStream(strFileNameInput, System.IO.FileMode.Open, System.IO.FileAccess.Read));
        }


        public void CalculateDuration()
        {   
            MediaMetadataRetriever mmr = new MediaMetadataRetriever();

            mmr.SetDataSource(m_strFileNameInput);

            String durationStr = mmr.ExtractMetadata(MetadataKey.Duration);
            m_dbDurationInSeconds = Convert.ToDouble(durationStr) / 1000f;

            mmr.Release();
            mmr.Dispose();
            mmr = null;
        }


        public void DoConversion()
        {
            Java.IO.RandomAccessFile fout;

            m_blFinished = false;
            m_ad = AudioDispatcherFactory.FromPipe(m_strFileNameInput, m_intTargetSampleRate, m_intBufferSize, 0);
            fout = new RandomAccessFile(m_strWaveFileNameOutput, "rw");
            m_wp = new WriterProcessor(m_ad.Format, fout);
            m_ad.AddAudioProcessor(m_wp);

            Task.Run(() =>
            {
                m_ad.Run();
            }).ContinueWith((encryptTask) => {
                m_blFinished = true;
                m_ad.Dispose();
                m_ad = null;
            });
        }


    }

}