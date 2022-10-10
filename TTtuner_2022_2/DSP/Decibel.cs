using BE.Tarsos.Dsp.DbLevel;
using System;

namespace TTtuner_2022_2.DSP
{
    internal sealed class Decibel
    {
        private DbLevelDetection m_db;
        internal Decibel()
        {
            m_db = new DbLevelDetection();
        }

        internal const double MIN_VALUE = -149.0f;
        internal const double MAX_VALUE = -0f;

        internal double? GetDbLevelFromShort(short[] shBuffer)
        {
            float[] floatData = new float[shBuffer.Length];
            double returnVale;

            for (int i = 0; i < shBuffer.Length; i++)
            {
                floatData[i] = (float)shBuffer[i] / (float)short.MaxValue;
            }
            returnVale = m_db.GetDbLevel(floatData);

            return Math.Max(MIN_VALUE, returnVale);
        }
    }
}