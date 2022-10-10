using System;
using System.Collections.Generic;
using System.Text;

using BE.Tarsos.Dsp.Pitch;

namespace TTtuner_2022_2.DSP
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using TTtuner_2022_2.Common;


    /// <summary>
    /// The class taken from java library https://github.com/JorenSix/TarsosDSP/blob/master/src/core/be/tarsos/dsp/pitch/McLeodPitchMethod.java
    /// </summary>
    internal sealed class MPM
    {

        internal const int DEFAULT_BUFFER_SIZE = 1024;
        private const double DEFAULT_CUTOFF = 0.97f;
        private const double SMALL_CUTOFF = 0.5f;
        private const double LOWER_PITCH_CUTOFF = 80.0; // Hz
        private readonly double cutoff;
        private readonly double sampleRate;
        private readonly double[] nsdf;
        private double turningPointX, turningPointY;
        private readonly IList<int?> maxPositions = new List<int?>();
        private readonly IList<double?> periodEstimates = new List<double?>();
        private readonly IList<double?> ampEstimates = new List<double?>();
        private McLeodPitchMethod m_mpm;
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: internal MPM(final double audioSampleRate)
        internal MPM(double audioSampleRate) : this(audioSampleRate, DEFAULT_BUFFER_SIZE, DEFAULT_CUTOFF)
        {
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: internal MPM(final double audioSampleRate, final int audioBufferSize)
        internal MPM(double audioSampleRate, int audioBufferSize) : this(audioSampleRate, audioBufferSize, DEFAULT_CUTOFF)
        {
        }

        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: internal MPM(final double audioSampleRate, final int audioBufferSize, final double cutoffMPM)
        internal MPM(double audioSampleRate, int audioBufferSize, double cutoffMPM)
        {
            this.sampleRate = audioSampleRate;
            nsdf = new double[audioBufferSize];
            this.cutoff = cutoffMPM;
            m_mpm = new McLeodPitchMethod((float)audioSampleRate, audioBufferSize, cutoffMPM);

        }

        internal double? getPitchFromShort(short[] data)
        {
            float[] floatData = new float[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                floatData[i] = (float)data[i] / (float)short.MaxValue;
            }

            PitchDetectionResult pitchRes = m_mpm.GetPitch(floatData);

            return (pitchRes.Pitch == -1 || pitchRes.Probability < Settings.NoteClarityFloat * 0.85f) ? null : (double?) pitchRes.Pitch;
        }

        internal double? getPitchFromFloat(float[] data)
        {
            PitchDetectionResult pitchRes = m_mpm.GetPitch(data);
            return (pitchRes.Pitch == -1) ? null : (double?)pitchRes.Pitch;
        }
    }
}
