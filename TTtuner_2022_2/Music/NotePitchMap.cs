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
using BE.Tarsos.Dsp.DbLevel;

namespace TTtuner_2022_2.Music
{
    internal class NotePitchMap
    {
        // internal enum TuningSytem { EQUAL_TEMPERAMENT, JUST_INTONATION }
        internal const double PITCH_LOW_LIMIT = 15.0f;
        internal const double PITCH_HIGH_LIMIT = 8000.0f;

        internal static string[] noteNames = new string[12];

        internal static string[] noteNames_default = new string[] { "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#", "A", "Bb", "B" };

        internal static double[] oct0 = new double[12];
        internal static double[] oct1 = new double[12];
        internal static double[] oct2 = new double[12];
        internal static double[] oct3 = new double[12];
        internal static double[] oct4 = new double[12];
        internal static double[] oct5 = new double[12];
        internal static double[] oct6 = new double[12];
        internal static double[] oct7 = new double[12];
        internal static double[] oct8 = new double[12];


        // pitch values for equal Temperament tuning system
        private static double[] oct0_ET = new double[] { 16.35, 17.32, 18.35, 19.45, 20.60, 21.83, 23.12, 24.50, 25.96, 27.50, 29.14, 30.87 };
        private static double[] oct1_ET = new double[] { 32.70, 34.65, 36.71, 38.89, 41.20, 43.65, 46.25, 49.001, 51.91, 55.001, 58.27, 61.74 };
        private static double[] oct2_ET = new double[] { 65.41, 69.30, 73.42, 77.78, 82.41, 87.31, 92.50, 98.001, 103.8, 110.001, 116.5, 123.5 };
        private static double[] oct3_ET = new double[] { 130.8, 138.6, 146.8, 155.6, 164.8, 174.6, 185.001, 196.001, 207.7, 220.001, 233.1, 246.9 };
        private static double[] oct4_ET = new double[] { 261.6, 277.2, 293.7, 311.1, 329.6, 349.2, 370.001, 392.001, 415.3, 440.001, 466.2, 493.9 };
        private static double[] oct5_ET = new double[] { 523.3, 554.4, 587.3, 622.3, 659.3, 698.5, 740.001, 784.001, 830.6, 880.001, 932.3, 987.8 };
        private static double[] oct6_ET = new double[] { 1047.001, 1109.001, 1175.001, 1245.001, 1319.001, 1397.001, 1480.001, 1568.001, 1661.001, 1760.001, 1865.001, 1976.001 };
        private static double[] oct7_ET = new double[] { 2093.001, 2217.001, 2349.001, 2489.001, 2637.001, 2794.001, 2960.001, 3136.001, 3322.001, 3520.001, 3729.001, 3951.001 };
        private static double[] oct8_ET = new double[] { 4186.001, 4435.001, 4699.001, 4978.001, 5274.001, 5588.001, 5920.001, 6272.001, 6645.001, 7040.001, 7459.001, 7902.001 };
        private static double[][] notes_EQ = new double[][] { oct0_ET, oct1_ET, oct2_ET, oct3_ET, oct4_ET, oct5_ET, oct6_ET, oct7_ET, oct8_ET };


        private static double[][] m_Notes = new double[][] { oct0, oct1, oct2, oct3, oct4, oct5, oct6, oct7, oct8 };

        private static double[] arrScaleCentsDeviations = new double[12];

        private const double ALLOWABLE_ERROR = 0.025;  // % of freq

        private const int NUM_OCTAVES = 9;
        private const int NUM_NOTES_PER_OCTAVE = 12;

        private static int m_intTranpositionOffset;

        private static double m_dlA4Ref;


        internal static int GetNoteMagnitude(string note)
        {
            //1. find where the note order without taking into account the octave
            string n1_Note = note.Substring(0, note.IndexOfAny("0123456789".ToCharArray()));
            string n1_Oct = note.Substring(note.IndexOfAny("0123456789".ToCharArray()), 1);


            for (int i = 0; i < NotePitchMap.noteNames_default.Length; i++)
            {
                if (NotePitchMap.noteNames_default[i] == n1_Note)
                {
                    return (Convert.ToInt32(n1_Oct) * 12) + i;
                }
            }

            return -1;
        }
        /// <summary>
        /// use this funciton to transpose all notes returned from this module to a certain offset
        /// </summary>
        /// <param name="intKeyOffset"></param>
        internal static void SetTranpositionOffset(int intKeyOffset)
        {
            m_intTranpositionOffset = intKeyOffset;
        }


        internal static void SetupTuningSystem(string strTuningSystem, int intRootScaleOffset, int intTransposeOffset, float flA4ref)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            string[] arrTemp = new string[12];
            double dlA4dev = flA4ref - 440;

            //   double dlPrctChange = (dlA4dev < 0 ?  -1f  : 1f  )   * dlA4dev / 440;
            double dlPrctChange = dlA4dev / 440;

            m_dlA4Ref = flA4ref;

            // set up tuning system

            if (strTuningSystem == "Equal Temperament")
            {
                comFunc.CopyArrays<double>(oct0_ET, ref oct0);
                comFunc.CopyArrays<double>(oct1_ET, ref oct1);
                comFunc.CopyArrays<double>(oct2_ET, ref oct2);
                comFunc.CopyArrays<double>(oct3_ET, ref oct3);
                comFunc.CopyArrays<double>(oct4_ET, ref oct4);
                comFunc.CopyArrays<double>(oct5_ET, ref oct5);
                comFunc.CopyArrays<double>(oct6_ET, ref oct6);
                comFunc.CopyArrays<double>(oct7_ET, ref oct7);
                comFunc.CopyArrays<double>(oct8_ET, ref oct8);


            }

            else
            {
                // load the cents deviations in the arrScaleCentsDeviations array
                LoadCentsDeviationArray(intRootScaleOffset);

                comFunc.CopyArrays<double>(GenerateScale(0), ref oct0);
                comFunc.CopyArrays<double>(GenerateScale(1), ref oct1);
                comFunc.CopyArrays<double>(GenerateScale(2), ref oct2);
                comFunc.CopyArrays<double>(GenerateScale(3), ref oct3);
                comFunc.CopyArrays<double>(GenerateScale(4), ref oct4);
                comFunc.CopyArrays<double>(GenerateScale(5), ref oct5);
                comFunc.CopyArrays<double>(GenerateScale(6), ref oct6);
                comFunc.CopyArrays<double>(GenerateScale(7), ref oct7);
                comFunc.CopyArrays<double>(GenerateScale(8), ref oct8);
            }


            // apply the a4 deviation changes
            for (int i = 0; i < m_Notes.Length; i++)
            {
                for (int j = 0; j < m_Notes[0].Length; j++)
                {
                    m_Notes[i][j] = (m_Notes[i][j] * dlPrctChange) + m_Notes[i][j];
                }
            }


            // set up transposition

            //comFunc.CopyArrays<string>(noteNames_default, ref arrTemp);


            //// rotate array algortihm

            //// reverse(A, 0, K - 1);
            //// reverse(A, K, A.length() - 1);
            //// reverse(A, 0, A.length() - 1);

            //int intRotationNumber = 12 - intTransposeOffset;

            //Array.Reverse(arrTemp, 0, intRotationNumber);
            //Array.Reverse(arrTemp, intRotationNumber, arrTemp.Length- (intRotationNumber) );
            //Array.Reverse(arrTemp, 0, arrTemp.Length );


            //comFunc.CopyArrays<string>(arrTemp, ref noteNames);


            // leave notes as default
            comFunc.CopyArrays<string>(noteNames_default, ref noteNames);


        }

        private static void LoadCentsDeviationArray(int intKeyOffset)
        {
            List<double> tuningSystemCentsDeviation = Common.Settings.TuningSystemCentsDeviation;
            // rotate cent devation array by the number of key offsets from the key A which scale values are based on
            // the EQ scales are baased on C so we must shift the cents deviation scale right by 9 to align and then rotate  
            // another 'intKeyOffset' positions
            int intNumOfRotations = 12 - (9 + intKeyOffset) % 12;


            // load the cents deviation array
            for (int i = 0; i < 12; i++)
            {
                arrScaleCentsDeviations[i] = tuningSystemCentsDeviation[i];
            }



            // to take into account the transposition settings
            //  find teh number of semiontes that the scale is transposed by
            // one semitone = 100 cents
            // Add (number of semitones offset * 100) cents to each element in the array
            // don't quite understand how this works but tested against other tuners and its how it should be implemented

            //for (int i = 0; i < 12; i++)
            //{
            //    arrScaleCentsDeviations[i] = arrScaleCentsDeviations[i] + (Common.Settings.TransposeOffset * 100 );
            //}
            int rotationsForTransposition = 12 - Common.Settings.TransposeOffset;
            Array.Reverse(arrScaleCentsDeviations, 0, rotationsForTransposition);
            Array.Reverse(arrScaleCentsDeviations, rotationsForTransposition, arrScaleCentsDeviations.Length - rotationsForTransposition);
            Array.Reverse(arrScaleCentsDeviations, 0, arrScaleCentsDeviations.Length);

            //rotate the array as neccessary

            // reverse(A, 0, K - 1);
            // reverse(A, K, A.length() - 1);
            // reverse(A, 0, A.length() - 1);

            Array.Reverse(arrScaleCentsDeviations, 0, intNumOfRotations);
            Array.Reverse(arrScaleCentsDeviations, intNumOfRotations, arrScaleCentsDeviations.Length - intNumOfRotations);
            Array.Reverse(arrScaleCentsDeviations, 0, arrScaleCentsDeviations.Length);
        }

        private static double[] GenerateScale(int intOctaveNumber)
        {

            double[] arrDbl = new double[12];
            int i;
            double dbValOfKeyOffset;


            for (i = 0; i < 12; i++)
            {
                dbValOfKeyOffset = notes_EQ[intOctaveNumber][i];
                // f2 = f1 * 2^( C / 1200 )
                arrDbl[i] = dbValOfKeyOffset * Math.Pow(2, arrScaleCentsDeviations[i] / 1200);
            }

            return arrDbl;
        }

        internal static bool AreNotesTheSameOrNeighbours(string strNote1, string strNote2, double dblPrctCloseness1, double dblPrctCloseness2)
        {

            bool isPrctClosnessLessThan100_Note1 = dblPrctCloseness1 < 100;
            bool isPrctClosnessLessThan100_Note2 = dblPrctCloseness2 < 100;

            int i, j;
            string[] noteNamesForAllOctaves = new string[NUM_NOTES_PER_OCTAVE * NUM_OCTAVES];

            for (i = 0; i < NUM_OCTAVES; i++)
            {
                for (j = 0; j < NUM_NOTES_PER_OCTAVE; j++)
                {
                    noteNamesForAllOctaves[i * NUM_NOTES_PER_OCTAVE + j] = noteNames[j] + i.ToString();
                }
            }

            if ((!noteNamesForAllOctaves.Contains(strNote1)) || (!noteNamesForAllOctaves.Contains(strNote2)))
            {
                return false;
            }

            if (strNote1 == strNote2)
            {
                return true;
            }

            if ((Math.Abs(Array.IndexOf(noteNamesForAllOctaves, strNote1) - Array.IndexOf(noteNamesForAllOctaves, strNote2)) > 1))
            {
                // notes are not neighbours in the scale
                return false;
            }


            if (Array.IndexOf(noteNamesForAllOctaves, strNote1) < Array.IndexOf(noteNamesForAllOctaves, strNote2))
            {
                if ((!isPrctClosnessLessThan100_Note1) && (isPrctClosnessLessThan100_Note2))
                {
                    return true;
                }
            }
            else
            {
                if ((isPrctClosnessLessThan100_Note1) && (!isPrctClosnessLessThan100_Note2))
                {
                    return true;
                }
            }

            return false;


        }



        internal static string GetNoteNameFromFreqency(int intNoteFrequencyVal)
        {
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct0[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct0[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct0, 0);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct1[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct1[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct1, 1);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct2[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct2[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct2, 2);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct3[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct3[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct3, 3);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct4[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct4[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct4, 4);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct5[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct5[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct5, 5);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct6[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct6[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct6, 6);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct7[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct7[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct7, 7);
            }
            if ((intNoteFrequencyVal >= (int)Math.Floor(NotePitchMap.oct8[0])) && (intNoteFrequencyVal <= (int)Math.Floor(NotePitchMap.oct8[NUM_NOTES_PER_OCTAVE - 1])))
            {
                return FindNoteInOctave(intNoteFrequencyVal, NotePitchMap.oct8, 8);
            }
            return string.Empty;

        }

        private static string FindNoteInOctave(int intNoteVal, double[] oct, int intOctNumber)
        {
            for (int i = 0; i < NUM_NOTES_PER_OCTAVE; i++)
            {
                if ((int)Math.Floor(oct[i]) == intNoteVal)
                {
                    return TransposeNote(NotePitchMap.noteNames[i], intOctNumber);
                }
            }

            return string.Empty;
        }


        internal static string TransposeNote(string strName, int intOctave)
        {
            int intTrasOffset = Common.Settings.TransposeOffset;
            Common.CommonFunctions comfunc = new Common.CommonFunctions();
            int intNoteOffset;
            // the way transpositon works is that it autaully specifies how many semitones the source instrument is above standard concert pitch.
            // Therefore we transpose the offset semitones DOWN to get the correct note.
            int intNoteIndexOutput;
            int intOuputOct = intOctave;
            intNoteOffset = comfunc.GetIndexOfItemInstringArray(noteNames, strName);
            intNoteIndexOutput = intNoteOffset;
            // make sure that transposition is possible

            if ((intOctave == 0) && (intNoteOffset - intTrasOffset < 0))
            {
                return "";
            }

            if ((intOctave == 8) && (intNoteOffset - intTrasOffset > 11))
            {
                return "";
            }

            for (int i = 0; i < intTrasOffset; i++)
            {
                if (intNoteIndexOutput == 0)
                {
                    intNoteIndexOutput = 12;
                    intOuputOct--;
                }
                intNoteIndexOutput--;
            }

            return noteNames[intNoteIndexOutput] + intOuputOct.ToString();


        }


        internal static string GetNoteFromPitch(double pitch, ref double centsDeviation)
        {
            // c = 1200 × 3.322038403 * log10 (f2 / f1)
            string outputNote = "";

            double[] comparisonOctave;
            double[] octave = null;
            double dlLowerLimit, dblUpperLimit;
            int bestFitOctave = 0;


            if ((pitch < PITCH_LOW_LIMIT) || (pitch > PITCH_HIGH_LIMIT))
            {
                // uiHelper.display(outputNote, percentCloseness);
                return outputNote;
            }
            else
            {
                for (int i = 0; i < m_Notes.Length; i++)
                {
                    comparisonOctave = m_Notes[i];
                    dlLowerLimit = comparisonOctave[0];
                    dblUpperLimit = comparisonOctave[11];

                    if ((pitch > (dlLowerLimit - (dlLowerLimit * ALLOWABLE_ERROR))) && (pitch < (dblUpperLimit + dblUpperLimit * ALLOWABLE_ERROR)))
                    {
                        octave = comparisonOctave;
                        bestFitOctave = i;
                        break;
                    }
                }
            }

            if (octave == null)
            {
                // uiHelper.display(outputNote, percentCloseness);
                return outputNote;
            }

            double bestDifference = 1000.0;
            int bestFitNoteIndex = -1;

            for (int i = 0; i < octave.Length; i++)
            {
                double diff = Math.Abs(pitch - octave[i]);
                if (diff < bestDifference)
                {
                    bestFitNoteIndex = i;
                    bestDifference = diff;
                }
            }
            // c = 1200 × 3.322038403 * log10 (f2 / f1)
            // http://www.sengpielaudio.com/calculator-centsratio.htm

            centsDeviation = 1200f * 3.322038403f * Math.Log10((pitch / octave[bestFitNoteIndex]));
            outputNote = TransposeNote(noteNames[bestFitNoteIndex], bestFitOctave);
            return outputNote;

        }


    }

}