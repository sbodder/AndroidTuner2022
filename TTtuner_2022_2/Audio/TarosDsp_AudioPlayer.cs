using BE.Tarsos.Dsp;
using BE.Tarsos.Dsp.IO.Android;
//using BE.Tarsos.Dsp.IO.Android;
using Java.Lang;
using System;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Audio
{
    internal class TarosDsp_AudioPlayer : IAudioPlayer
    {

        private System.IO.FileStream m_fsInput;
        private int m_intSampleRate;
        private long m_numChannels;
        const int WAVE_HEADER_SIZE_BYTES = 44;
        WaveformSimilarityBasedOverlapAdd m_wsola;
        AudioDispatcher m_ad = null;
        AndroidAudioPlayer m_ap;
        double m_speed;


        internal TarosDsp_AudioPlayer()
        { }

        public int Duration
        {
            get { return 0; }
        }
        public bool IsPlaying { get; set; }

        public int CurrentPosition
        {
            get { return 0; }
        }

        // float Speed { get; }

        public void SeekTo(int intPosition)
        { }

        public void SetupPlayer(string strFilename, float flSpeed, bool blStartPlayAfterSetup, int intPositionToStartFrom = 0)
        {
            WaveformSimilarityBasedOverlapAdd.Parameters parameters;
            short shNumChannels;
            byte[] arrHeader = new byte[WAVE_HEADER_SIZE_BYTES];
            if (m_fsInput != null)
            {
                m_fsInput.Close();
                m_fsInput.Dispose();
            }

            try
            {
                //m_input = new DataInputStream(new System.IO.FileStream(strFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                m_fsInput = new System.IO.FileStream(strFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                //m_rawData = new byte[(int)intFlLength];
                //m_input.Read(m_rawData, 0, (int)intFlLength);


                m_fsInput.Read(arrHeader, 0, (int)WAVE_HEADER_SIZE_BYTES);
            }
            catch (System.Exception e1)
            {
                throw new System.Exception(e1.Message);
            }


            try
            {
                // android is little endian
                m_intSampleRate = arrHeader[24] + (arrHeader[25] * 256) + (arrHeader[26] * 256 * 256) + (arrHeader[27] * 256 * 256 * 256);

                m_speed = flSpeed;
                //parameters = WaveformSimilarityBasedOverlapAdd.Parameters.MusicDefaults(0.1, m_intSampleRate);
                parameters = WaveformSimilarityBasedOverlapAdd.Parameters.MusicDefaults(m_speed, 48000);


#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In SetupPlayer : speed is :" + m_speed);
#endif
                m_wsola = new WaveformSimilarityBasedOverlapAdd(parameters);

                shNumChannels = BitConverter.ToInt16(arrHeader, 22);
                m_numChannels = (long)shNumChannels;

               // m_ad = AudioDispatcherFactory.FromPipe(strFilename, m_intSampleRate, System.Math.Max(5000, m_wsola.InputBufferSize), 0);
                m_ad = AudioDispatcherFactory.FromPipe(strFilename, 48000, System.Math.Max(5000, m_wsola.InputBufferSize), 0);

                m_ap = new AndroidAudioPlayer(m_ad.Format);
                m_ad.AddAudioProcessor(m_wsola);
                m_ad.AddAudioProcessor(m_ap);
                //Thread t = new Thread(m_ad);
                //t.Start();
                m_ad.Run();
            }
            catch (System.Exception e1)
            {
                throw new System.Exception(e1.Message);
            }

        }

        public void Pause(bool stopPlay = true)
        { }


        public void Play()
        { }

        public void Stop()
        { }

        public void Destroy()
        { }

        public void ChangeSpeed(float flSpeed)
        { }
    }
}