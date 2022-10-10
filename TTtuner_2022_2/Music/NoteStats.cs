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
using TTtuner_2022_2.Plot;
using System.Collections.ObjectModel;
using System.Globalization;
using Android.Graphics;
using System.IO;
using System.Reflection;
using Syncfusion.SfDataGrid;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TTtuner_2022_2.Common;
using System.Text.RegularExpressions;
using Java.Util.Regex;

namespace TTtuner_2022_2.Music
{

    public class NoteStatsGenerator
    {
        private Java.Util.Regex.Pattern _pNoteAndOct = Java.Util.Regex.Pattern.Compile("([A-Za-z#]{1,2})([0-9])");
        public NoteStatsGenerator(int minimumNoOfSamples)
        {
            MIN_NUMBER_OF_SAMPLES = minimumNoOfSamples;
        }
        //ObservableCollection<NoteStats> Stats { get; set; }
        private int MIN_NUMBER_OF_SAMPLES;

        public class NoteStat : INotifyPropertyChanged
        {            
            private Java.Util.Regex.Pattern _pImageOct = Java.Util.Regex.Pattern.Compile("Image([0-9])");
            public const int NUM_OCTAVES = 9;
            //private double m_dbTotalCentsError;
            private const double OK_UPPER_THRESHOLD = 10d;
            private const double HIGH_ERROR_LOWER_THRESHOLD = 25d;
            public event PropertyChangedEventHandler PropertyChanged;

            private List<List<double>> m_lstCentDeviations = new List<List<double>>();
            private List<int> _lstSamples = new List<int>();
            private List<double> _lstDbTotalCentsError = new List<double>();

            public string Note { get; set; }

            public NoteStat()
            {
                SetupLists();
            }

            public NoteStat(string note)
            {
                Note = note;
                SetupLists();
            }

            private void SetupLists()
            {
                for (int i = 0; i < NUM_OCTAVES; i++)
                {
                    m_lstCentDeviations.Add(new List<double>());
                    _lstSamples.Add(0);
                    _lstDbTotalCentsError.Add(0);
                }
            }

            public void DeleteData()
            {
                _lstSamples.Clear();
                _lstDbTotalCentsError.Clear();

                for (int i = 0; i < NUM_OCTAVES; i++)
                {
                    m_lstCentDeviations[i].Clear();
                    _lstSamples.Add(0);
                    _lstDbTotalCentsError.Add(0);
                }

            }

            public int GetSamples(int oct)
            {
                return _lstSamples[oct];
            }

            public string AverageCentsError0
            {
                get
                {                    
                    double dblOffset = (_lstDbTotalCentsError[0] / _lstSamples[0]);
                    return _lstSamples[0] == 0 || _lstSamples[0] < Settings.MinNumberOfSamplesForNote  ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble0
            {
                get
                {
                    if (_lstSamples[0] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[0] / _lstSamples[0]);
                    return dblOffset;
                }
            }


            public string AverageCentsError1
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[1] / _lstSamples[1]);
                    return _lstSamples[1] == 0 || _lstSamples[1] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble1
            {
                get
                {
                    if (_lstSamples[1] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[1] / _lstSamples[1]);
                    return dblOffset;
                }
            }


            public string AverageCentsError2
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[2] / _lstSamples[2]);
                    return _lstSamples[2] == 0 || _lstSamples[2] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble2
            {
                get
                {
                    if (_lstSamples[2] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[2] / _lstSamples[2]);
                    return dblOffset;
                }
            }


            public string AverageCentsError3
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[3] / _lstSamples[3]);
                    return _lstSamples[3] == 0 || _lstSamples[3] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble3
            {
                get
                {
                    if (_lstSamples[3] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[3] / _lstSamples[3]);
                    return dblOffset;
                }
            }


            public string AverageCentsError4
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[4] / _lstSamples[4]);
                    return _lstSamples[4] == 0 || _lstSamples[4] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble4
            {
                get
                {
                    if (_lstSamples[4] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[4] / _lstSamples[4]);
                    return dblOffset;
                }
            }


            public string AverageCentsError5
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[5] / _lstSamples[5]);
                    return _lstSamples[5] == 0 || _lstSamples[5] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble5
            {
                get
                {
                    if (_lstSamples[5] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[5] / _lstSamples[5]);
                    return dblOffset;
                }
            }


            public string AverageCentsError6
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[6] / _lstSamples[6]);
                    return _lstSamples[6] == 0 || _lstSamples[6] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble6
            {
                get
                {
                    if (_lstSamples[6] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[6] / _lstSamples[6]);
                    return dblOffset;
                }
            }


            public string AverageCentsError7
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[7] / _lstSamples[7]);
                    return _lstSamples[7] == 0 || _lstSamples[7] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble7
            {
                get
                {
                    if (_lstSamples[7] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[7] / _lstSamples[7]);
                    return dblOffset;
                }
            }


            public string AverageCentsError8
            {
                get
                {
                    double dblOffset = (_lstDbTotalCentsError[8] / _lstSamples[8]);
                    return _lstSamples[8] == 0 || _lstSamples[8] < Settings.MinNumberOfSamplesForNote ? "-" : FormatCentOutput(dblOffset);
                }
            }

            public double AverageCentsErrorDouble8
            {
                get
                {
                    if (_lstSamples[8] < Settings.MinNumberOfSamplesForNote)
                    {
                        return 0f;
                    }
                    double dblOffset = (_lstDbTotalCentsError[8] / _lstSamples[8]);
                    return dblOffset;
                }
            }

            public object Clone()
            {
                return this.MemberwiseClone();
            }


            public void TotalCentsError_Add(int oct, double increase)
            {
                _lstDbTotalCentsError[oct] += increase;
                m_lstCentDeviations[oct].Add(increase);

            }

            public void Samples_Increase(int oct)
            {
                _lstSamples[oct]++;

            }

            internal double LowerInterQuartileRange( int oct)
            {
                return FindInterQuartileValue(oct, 0.25f);                
            }

            internal double UpperInterQuartileRange(int oct)
            {
               return FindInterQuartileValue(oct, 0.75f);
                
            }

             

            internal Bitmap DrawBitmap(int octave)
            {
                float x1, y1, x2, y2;
                const int PADDING = 5;

                const int BITMAP_WIDTH = 500;
                // to calc the mult factor. Remember the max cents deviation in either direction is =/-50
                // so the nult factor will be half the bitmap width divided by 50
                // it would be uncommon to get an average cent dev of 50 so we divide by a number less than 50 so that 
                // the effect is empahised
                const float MULT_FACTOR = (BITMAP_WIDTH / 2) / 50;
                const int BITMAP_HEIGHT = 200;
                const int Y_PADDING = 30;
                const float CIRCLE_RADIUS = 20f;
                Bitmap bitmap = Bitmap.CreateBitmap(BITMAP_WIDTH, BITMAP_HEIGHT, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(bitmap);
                Paint paint = new Paint();


                canvas.DrawARGB(0, 0, 0, 0);
                paint.AntiAlias = true;
                paint.SetStyle(Paint.Style.Fill);


                // draw rectangle
                //Type t = this.GetType();
                //PropertyInfo pi = this.GetType().GetProperty($"AverageCentsErrorDouble{octave}");
                double averageCentsErrorDouble = (double)this.GetType().GetProperty($"AverageCentsErrorDouble{octave}").GetValue(this);
                if (Math.Abs(averageCentsErrorDouble) > OK_UPPER_THRESHOLD && Math.Abs(averageCentsErrorDouble) < HIGH_ERROR_LOWER_THRESHOLD)
                {
                    paint.Color = Color.Yellow;
                }
                else if (Math.Abs(averageCentsErrorDouble) > HIGH_ERROR_LOWER_THRESHOLD)
                {
                    paint.Color = Color.Red;
                }
                else
                {
                    paint.Color = Color.Green;
                }

                x1 = (float)(BITMAP_WIDTH / 2 + FindInterQuartileValue(octave, 0.25f) * MULT_FACTOR);
                if (x1 < 0)
                {
                    x1 = 0;
                }
                y1 = Y_PADDING;

                x2 = (float)(BITMAP_WIDTH / 2 + FindInterQuartileValue(octave, 0.75f) * MULT_FACTOR);

                if (x2 > BITMAP_WIDTH)
                {
                    x2 = BITMAP_WIDTH;
                }
                y2 = BITMAP_HEIGHT - Y_PADDING;

                canvas.DrawRect(x1, y1, x2, y2, paint);

                // draw  circle
                paint.Color = Color.Black;
                x1 = (float)(BITMAP_WIDTH / 2 + averageCentsErrorDouble * MULT_FACTOR);
                y1 = BITMAP_HEIGHT / 2;
                canvas.DrawCircle(x1, y1, CIRCLE_RADIUS, paint);


                // draw center line
                paint.Color = Color.Gray;
                paint.SetStyle(Paint.Style.Stroke);
                paint.StrokeWidth = 10f;

                canvas.DrawLine(BITMAP_WIDTH / 2, 0, BITMAP_WIDTH / 2, BITMAP_HEIGHT, paint);

                // draw circumference lines

                canvas.DrawLine(PADDING, 0, PADDING, BITMAP_HEIGHT, paint);
                canvas.DrawLine(BITMAP_WIDTH - PADDING, 0, BITMAP_WIDTH - PADDING, BITMAP_HEIGHT, paint);

                canvas.DrawLine(0, 0, BITMAP_WIDTH, 0, paint);
                canvas.DrawLine(0, BITMAP_HEIGHT, BITMAP_WIDTH, BITMAP_HEIGHT, paint);

                canvas.Dispose();
                canvas = null;
                paint.Dispose();
                paint = null;

                return bitmap;
            }


            public Bitmap Image0
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image1
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image2
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image3
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image4
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image5
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image6
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image7
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }

            public Bitmap Image8
            {
                get
                {
                    int? currOct = GetOctaveOfImagePropertyName(MethodBase.GetCurrentMethod().Name);
                    return currOct != null ? DrawBitmap(currOct ?? default(int)) : null;
                }
                set
                {

                }
            }


            private int? GetOctaveOfImagePropertyName(string imagePropName)
            {
                Matcher matcher = _pImageOct.Matcher(imagePropName);
                if (matcher.Find())
                {
                    return Int16.Parse(matcher.Group(1));
                }
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="percent"> Should be between 0 and 1</param>
            /// <returns></returns>
            private double FindInterQuartileValue(int oct, float percent)
            {
                int intValNumber;

                if ((percent < 0) || (percent > 1))
                {
                    throw new Exception("FindQuartileValue function - value passed is not between 0 and 1");
                }

                intValNumber = (int)(m_lstCentDeviations[oct].Count * percent);
                m_lstCentDeviations[oct] = m_lstCentDeviations[oct].OrderBy(dbl => dbl).ToList();

                return m_lstCentDeviations[oct].Count > 0 && _lstSamples[oct] >= Settings.MinNumberOfSamplesForNote ? m_lstCentDeviations[oct][intValNumber] : 0f ;
            }

            public MemoryStream LoadResource(String Name)
            {
                MemoryStream memory = new MemoryStream();


                var assembly = Assembly.GetExecutingAssembly();

                //string[] resources = assembly.GetManifestResourceNames();

                var path = String.Format("TTtuner_2022_2.Resources.drawable.{0}", Name);

                var aStream = assembly.GetManifestResourceStream(path);

                aStream.CopyTo(memory);

                return memory;
            }

            public void OnPropertyChanged([CallerMemberName] string name = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }


            internal void Merge(NoteStat src)
            {
                
                for (int i = 0; i < NoteStat.NUM_OCTAVES; i++)
                {
                    if (src.GetSamples(i) > 0)
                    {
                        m_lstCentDeviations[i].AddRange(src.m_lstCentDeviations[i]);
                        _lstSamples[i] += src.GetSamples(i);
                        _lstDbTotalCentsError[i] += src._lstDbTotalCentsError[i];
                        OnPropertyChanged($"AverageCentsError{i}");
                        OnPropertyChanged($"Image{i}");
                    }
                }
            }

            private string FormatCentOutput(double val)
            {
                StringBuilder builder = new StringBuilder();
                string fmt = "00";
                if (val > 0)
                {
                    builder.Append("+");
                }
                builder.Append(val.ToString(fmt, CultureInfo.InvariantCulture));
                return builder.ToString();
            }

        }

        internal bool MergeNoteStatCollections(ObservableCollection<NoteStat> dest, ObservableCollection<NoteStat> src)
        {
            bool noteAdded = false;
            foreach (NoteStat ns in src)
            {
                var nsTemp = dest.AsEnumerable().Where(stat => stat.Note == ns.Note);
                if (nsTemp.Any())
                {
                    nsTemp.FirstOrDefault().Merge(ns);
                }
                else
                {
                    noteAdded = true;
                    dest.Add(ns);
                }
            }

            return noteAdded;
        }

        internal ObservableCollection<NoteStat> GenerateStats_Freq(ObservableCollection<ISerialisableDataPoint> lstInput)
        {
            ObservableCollection<NoteStat> lstStatsRet = new ObservableCollection<NoteStat>();
            NoteStat ntTemp;
            string strNoteName, strNote;
            int oct;
            double dlCentError = 0;

            List<Serializable_DataPoint> lstInput_frq = lstInput.ToList().ConvertAll(o => (Serializable_DataPoint)o);

            foreach (Serializable_DataPoint dp in lstInput_frq)
            {

                strNoteName = NotePitchMap.GetNoteFromPitch(dp.YVal, ref dlCentError);

                // find the octave and note string 
                (strNote, oct) = this.GetNoteNameAndOctave(strNoteName);
                if (oct== -1) { continue; }
                if (lstStatsRet.AsEnumerable().Where(stat => stat.Note == strNote).Any())
                {
                    // note already exists in ObservableCollection
                    ntTemp = lstStatsRet.AsEnumerable().Where(stat => stat.Note == strNote).FirstOrDefault();
                    ntTemp.Samples_Increase(oct);
                    ntTemp.TotalCentsError_Add(oct, dlCentError);

                }
                else
                {
                    ntTemp = new NoteStat();
                    ntTemp.Samples_Increase(oct);
                    ntTemp.TotalCentsError_Add(oct, dlCentError);
                    ntTemp.Note = strNote;
                    lstStatsRet.Add(ntTemp);
                }

            }

            // don't return any notes where the samples are less or equal to MIN_NUMBER_OF_SAMPLES 
            return new ObservableCollection<NoteStat>(lstStatsRet.AsEnumerable());
        }

        internal string GetStatsAsString(DataPointHelper<Serializable_DataPoint> dataHelperFrq, DataPointHelper<Serializable_DataPoint_Std> dataHelperDcb = null)
        {
            // this will be a text file comprimised of the difference cvs files.
            // 1. The scale used (curret scale selected in the program
            // EG Temperament | A | Bb | B | C | C#|D|Eb|E|F|F#|G|G#
            //    Bendeler (Fractions)|0|6.546|3.91|10.456|0.681|-1.955|4.591|1.955|8.501|-1.274|3.229|2.636
            // 2. The Stats of the recording
            // eg STATS A2 | A3 | A5 | B6 | D5 | etc (average cents deviation)
            //          -20.1 | 19 | 15.6 | 3 | etc
            // 3 RAW DATA etc                

            CommonFunctions comFun = new CommonFunctions();
            ObservableCollection<NoteStat> lstNoteStats;
            ObservableCollection<DbStatsGenerator.DbStat> lstDbStats = null;
            NoteStatsGenerator frqStat = new NoteStatsGenerator(Common.Settings.MinNumberOfSamplesForNote);
            DbStatsGenerator dbStat = new DbStatsGenerator();
            List<double> dbLst = Settings.TuningSystemCentsDeviation;
            string strReturn = "";
            bool outputDcb = false;
            Type t_ns= new NoteStat().GetType();

            lstNoteStats = frqStat.GenerateStats_Freq(dataHelperFrq.DataPoints);

            if (dataHelperDcb != null)
            {
                outputDcb = true;
                lstDbStats = dbStat.GenerateStats_Db(dataHelperDcb.DataPoints);
            }

            // output current tuning system
            strReturn += "////////////////////// TUNING SYSTEM ///////////////////////////////////\n";
            strReturn += string.Format("\nTuning System : {0}\n\n", Settings.TuningSystemAndRootScale);
            strReturn += "The following values give the Cents deviation from each note compared to Equal Temperament: \n";
            strReturn += "A (Cents),Bb (Cents),B (Cents),C (Cents),C# (Cents),D (Cents),Eb (Cents),E (Cents),F (Cents),F# (Cents),G (Cents),G# (Cents)\n";
            strReturn += string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00}\n", dbLst[0], dbLst[1], dbLst[2]
                , dbLst[3], dbLst[4], dbLst[5], dbLst[6], dbLst[7], dbLst[8], dbLst[9], dbLst[10], dbLst[11]);

            // output stats
            strReturn += "\n////////////////////// STATS ///////////////////////////////////\n";

            strReturn += "\nNote,Samples,Average Cents Error (Cents),LowerInterQuartile (Cents),UpperInterQuartile (Cents)\n";
            foreach (NoteStatsGenerator.NoteStat ns in lstNoteStats)
            {
                for (int i = 0; i < NoteStat.NUM_OCTAVES; i++)
                {                    
                    if (ns.GetSamples(i) > 0)
                    {
                        strReturn += string.Format(
                            "{0},{1},{2},{3:0.00},{4:0.00}\n",
                            ns.Note + i.ToString(),
                            ns.GetSamples(i),
                            t_ns.GetProperty($"AverageCentsError{i}").GetValue(ns),
                            ns.LowerInterQuartileRange(i),
                            ns.UpperInterQuartileRange(i)
                            );
                    }
                }
            }

            if (outputDcb)
            {
                strReturn += "\n\nMean(dB),Samples,95th Percentile(dB),LowerInterQuartile(dB),UpperInterQuartile (dB)\n";
                strReturn += string.Format(
                            "{0},{1},{2},{3:0.00},{4:0.00}\n",
                            lstDbStats[0].Mean,
                            lstDbStats[0].Samples,
                            lstDbStats[0].Percentile95,
                            lstDbStats[0].LowerInterQuartileRange,
                            lstDbStats[0].UpperInterQuartileRange
                            );
            }

            // output raw data
            strReturn += "\n////////////////////// RAW DATA ///////////////////////////////////\n";
            strReturn += "\nX (seconds),Y (Hertz),Note,Cents Error (Cents)\n";
            foreach (Serializable_DataPoint dp in dataHelperFrq.DataPoints.OrderBy(dp => dp.XVal))
            {
                strReturn += string.Format("{0:0.00},{1:0.00},{2},{3:0.00}\n", dp.XVal, dp.YVal, dp.Note, dp.PctCloseness);
            }

            if (outputDcb)
            {
                strReturn += "\n\nX (seconds),Y (dB)\n";
                foreach (Serializable_DataPoint_Std dp in dataHelperDcb.DataPoints)
                {
                    strReturn += string.Format("{0:0.00},{1:0.00}\n", dp.XVal, dp.YVal);
                }
            }

            return strReturn;

        }

        private (string, int) GetNoteNameAndOctave(string note)
        {
            //// User regex to find the octave of the current prop called
            Matcher matcher = _pNoteAndOct.Matcher(note);
            if (matcher.Find())
            {
                return (matcher.Group(1), Int16.Parse(matcher.Group(2)));
            }
            else
            {
                return (null, -1);
            }
        }

    }
}