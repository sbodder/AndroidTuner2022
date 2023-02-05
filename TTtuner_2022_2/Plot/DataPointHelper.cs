using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using global::Android.Views;
using global::Android.Widget;
using TTtuner_2022_2.Common;
using TTtuner_2022_2.DSP;
using TTtuner_2022_2.Music;
using Java.Util;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

namespace TTtuner_2022_2.Plot
{

    internal class DataPointCollection
    {
        public static DataPointHelper<Serializable_DataPoint> Frq = 
            new DataPointHelper<Serializable_DataPoint>(
                                                        "Hertz", 
                                                        NotePitchMap.PITCH_LOW_LIMIT, 
                                                        NotePitchMap.PITCH_HIGH_LIMIT, false);

        public static DataPointHelper<Serializable_DataPoint_Std> Dcb = 
            new DataPointHelper<Serializable_DataPoint_Std>(
                                                        "dB",
                                                        Decibel.MIN_VALUE, 
                                                        Decibel.MAX_VALUE, false );

        public static void ClearAllDataPoints()
        {
            DataPointCollection.Frq.ClearData();
            DataPointCollection.Dcb.ClearData();
        }
    }


    internal class DataPointHelper<T> : IDataPointHelper where T : new()
    {
        public ObservableCollection<ISerialisableDataPoint> _datapoints = new ObservableCollection<ISerialisableDataPoint>();
        string _unit;
        double _yMin, _yMax;
        bool _bMoving;
#if Trial_Version
        const int MAX_POINTS_FOR_MOVING_WINDOW = 50; 
#else
        const int MAX_POINTS_FOR_MOVING_WINDOW = 2000; 
#endif



        //   DataPoints;
        internal ObservableCollection<ISerialisableDataPoint> DataPoints
        {
            get { return _datapoints as ObservableCollection<ISerialisableDataPoint>; }
        }

        public int NumberOfDataPoints { get { return _datapoints.Count; } }

        internal void ClearData()
        {
            _datapoints.Clear();
        }

        internal DataPointHelper(string unit, double yMin, double yMax, bool bMoving)
        {
            _unit = unit;
            _yMin = yMin;
            _yMax = yMax;
            _bMoving = bMoving;
        }

        public void AddDataPointToCollection(ISerialisableDataPoint dp)
        {
            if (_bMoving && _datapoints.Count >= MAX_POINTS_FOR_MOVING_WINDOW)
            {
                _datapoints.RemoveAt(0);
            }
            if ((dp.YVal > _yMin) && (dp.YVal < _yMax))
            {
                //AddDataPointToCollection(dp.XVal, dp.YVal);
                _datapoints.Add(dp);
            }
        }

        internal void DiscardDataPointsWhereXlessThan(double cuttoffPoint_X)
        {
            _datapoints = new ObservableCollection<ISerialisableDataPoint>(_datapoints.Where(
                                                                                            p => p.XVal > cuttoffPoint_X));
        }

        public void AddDataPointToCollection(double x, double y)
        {
            T dp = new T();
            (dp as ISerialisableDataPoint).XVal = x;
            (dp as ISerialisableDataPoint).YVal = y;
            (dp as ISerialisableDataPoint).Recalculate();
            _datapoints.Add((dp as ISerialisableDataPoint));
        }

        public bool SaveDataPointsToFile(string strFileName)
        {
            var documentsPath = Common.Settings.DataFolder;
            var filePath = System.IO.Path.Combine(documentsPath, strFileName);

            try
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In SaveDataPointsToFile in pitchplothellper....");

                //Logger.Info(Common.CommonFunctions.APP_NAME, "there are " + m_dictPointCollection[FRQ_I].Count + " points in PitchPoints");
#endif
                long fileLength;
                List<T> lsDp = new List<T>();
                List<T> orderderlsDp = new List<T>();
                //(d_item.Value.TypeOfDatapoint) lsDp = new List<ISerialisableDataPoint>();
                if (_datapoints.Count <= 10)
                {
                    // not enough data points to make this worth while saving
                    return false;
                }

                foreach (ISerialisableDataPoint point in _datapoints)
                {
                    lsDp.Add((T)point);
                }

                orderderlsDp = lsDp.OrderBy(n => (n as ISerialisableDataPoint).XVal).ToList();

                using (Stream stream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(stream, orderderlsDp);
                    fileLength = stream.Length;
                   //stream.Length;
                }


                using (System.IO.Stream stream = FileHelper.OpenFileOutputStream(strFileName, true,  fileLength))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    bformatter.Serialize(stream, orderderlsDp);
                }

                return true;
            }
            catch (Exception e1)
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "Exception in SaveDataPointsToFile:" + e1.Message);
#endif
                throw e1;
            }
        }


        internal void LoadDataPointsFromFile(string strFilePath, bool blRecalculateNotes = false)
        {
            
            List<T> lsDp = new List<T>();

            ClearData();

#if Release_LogOutput
            // clear current line series
            Logger.Info(Common.CommonFunctions.APP_NAME, "In LoadDataPointsFromFile in data point helper activity: strFilePath is :" + strFilePath);
#endif
            try
            {

                using (System.IO.Stream stream = FileHelper.OpenFileInputStream(strFilePath))
                {

                    BinaryFormatter bf = new BinaryFormatter();

                    // deserialize

                    bf.Binder = new BindChanger();
                    lsDp = (List<T>)bf.Deserialize(stream);

#if DEBUG
                    Logger.Info(Common.CommonFunctions.APP_NAME, "////////////////////////////////DATA POINTS SPIT OUT/////////////////////////////////////////");
                    Logger.Info(Common.CommonFunctions.APP_NAME, (lsDp[0] as ISerialisableDataPoint).PrintHeader(_unit));
#endif
                }

                Logger.Info(Common.CommonFunctions.APP_NAME, "In LoadDataPointsFromFile in data point activity: number of points in opened text file  is : " + lsDp.Count);
                foreach (ISerialisableDataPoint point in lsDp)
                {
                    if (blRecalculateNotes)
                    {
                        point.Recalculate();
                    }
                    AddDataPointToCollection(point);
                }

            }
            catch (Exception e1)
            {
                Logger.Info(Common.CommonFunctions.APP_NAME, "In LoadDataPointsFromFile, Exception :  " + e1.Message);
                throw new Exception("Exception when opeing the text file : " + e1.Message);
            }

        }
    }

    class BindChanger : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        { 
            // Get the current assembly
            string currentAssembly = Assembly.GetExecutingAssembly().FullName;

            var newType = typeName.Replace("TuneTracker", "TTtuner_2022_2");

            // Create the new type and return it
            //typeToDeserialize = Type.GetType(string.Format("{0}, {1}", newType, currentAssembly));

            var typeToDeserialize = Type.GetType(newType);

            return typeToDeserialize;
        }
    }
}