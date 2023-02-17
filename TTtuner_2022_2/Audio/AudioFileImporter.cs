

using Android.App;
using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TTtuner_2022_2.Common;
using TTtuner_2022_2.Plot;

namespace TTtuner_2022_2.Audio
{
    internal class AudioFileImporter
    {
        const int WAVE_HEADER_SIZE_BYTES = 44;
        Activity m_act;

        const int CONVERT_PROGESS_TOTAL = 10;

        const int FILE_GENERATION_PROGESS_TOTAL = 100 - CONVERT_PROGESS_TOTAL;

        const int CONVERT_PROGESS_PERCENT_INCREMENTS = 30;

        const int FILE_GENERATION_PROGESS_PERCENT_INCREMENTS = 3;

        private int m_numFiles;

        internal event EventHandler<FileProgressArgs> FileProgress;

        internal class FileProgressArgs : EventArgs
        {
            private int percentOfFileCompleted;
            private int numOfFilesCompleted;
            bool blProcessingComplete;


            internal FileProgressArgs(int pPercentOfFileCompleted, int pNumOfFilesCompleted, bool complete)
            {
                percentOfFileCompleted = pPercentOfFileCompleted;
                numOfFilesCompleted = pNumOfFilesCompleted;
                blProcessingComplete = complete;
            }

            internal int PercentOFFileComplete { get { return percentOfFileCompleted; } }

            internal int NumberOfFilesCompleted { get { return numOfFilesCompleted; } }

            internal bool Complete { get { return blProcessingComplete; } }

        }

        internal class ImportFile
        {
            internal ImportFile(string fileName, bool convert)
            {
                FileName = fileName;
                ConvertFrom32to16bit = convert;
            }
            internal string FileName { get; set; }
            internal bool ConvertFrom32to16bit { get; set; }
        }

        public delegate double? AudioDataProcessorDelegate(short[] shData);

        internal class DataProcessorAndStore
        {
            internal AudioDataProcessorDelegate _pFuncAudioProcessor; // like the mpm. getPitchFromShort function
            internal IDataPointHelper _dpHlp;//Will store the data here
            // mult of the freq to read data (per sec) . The freq of the read will 
            // be passed into the fuction so you can mod an iterator count
            // to know when to read the data. Ie a value of 2 means that the 
            // data will be read only every second time of the iterator
            internal int _samplingFreqDivBy;
            internal string unit;
            internal DataProcessorAndStore(AudioDataProcessorDelegate pFunc, IDataPointHelper dtHelper, int sampleFreqDivBy, string strUnit)
            {
                _pFuncAudioProcessor = pFunc;
                _dpHlp = dtHelper;
                _samplingFreqDivBy = sampleFreqDivBy;
                unit = strUnit;
            }
        }


        internal AudioFileImporter()
        {
        }

        internal void ImportWaveFiles(Activity act, string[] lstFileNames, string strImportDir)
        {
            int intBitDepth = 0, intFormatCode = 0, numChannels = 0;
            bool blDoImport = true;
            AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(act);
            int i;
            List<ImportFile> lstFilesToImport = new List<ImportFile>();
            m_numFiles = lstFileNames.Length;

            m_act = act;

            for (i = 0; i < lstFileNames.Length; i++)
            {
                Common.CommonFunctions comFunc = new Common.CommonFunctions();

                string fileNAme = comFunc.GetFileNameFromPath(lstFileNames[i]);

                string path = System.IO.Path.Combine(Common.Settings.DataFolder, fileNAme);

                if (FileHelper.CheckIfFileExists(path, false))
                {
                    dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(act);

                    dlMessage.SetTitle("Import Error");
                    dlMessage.SetMessage("The file '" + lstFileNames[i] + "' cannot be imported. The file name already exists in the destination folder");
                    dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                    blDoImport = false;

                    continue;
                }
                lstFilesToImport.Add(new ImportFile(lstFileNames[i], true));
            }


            if (blDoImport)
            {
                Common.CommonFunctions comFunc = new Common.CommonFunctions();
                try
                {
                    ImportFilesToDataFolder(lstFilesToImport);
                }
                catch (Exception e)
                {
                    dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(act);

                    dlMessage.SetTitle("Import Error");
                    dlMessage.SetMessage(comFunc.TruncateStringRight(e.Message, 200));
                    dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });

                    //this will close the file progress dialog
                    FileProgressArgs filep = new FileProgressArgs(100, 0, true);
                    FileProgress?.Invoke(this, filep);

                    act.RunOnUiThread(() =>
                    {
                        dlMessage.Show();
                    });
                }
            }
            else
            {
                //this will close the file progress dialog
                FileProgressArgs filep = new FileProgressArgs(100, 0, true);
                FileProgress?.Invoke(this, filep);

                act.RunOnUiThread(() =>
                {
                    dlMessage.Show();
                });
            }
        }

        internal void GenerateDecibelFile(string strWaveFilePath, int indexOfFile)
        {
            DSP.Decibel db = new DSP.Decibel();
            int SAMPLES = Common.Settings.NumberOfSamplesInBuffer;
            DataPointHelper<Serializable_DataPoint_Std> dpHlp = DataPointCollection.Dcb;
            int samplingFreq;
            long lgLength = Common.FileHelper.GetLengthOfFile(strWaveFilePath);
            Common.CommonFunctions comFunc = new Common.CommonFunctions();


            DataProcessorAndStore dPandS = new DataProcessorAndStore(
                                           new AudioDataProcessorDelegate(db.GetDbLevelFromShort),
                                           dpHlp,
                                           1, "dB");

            // find out how many samples we need per sec
            samplingFreq = Common.Settings.PitchSamplingFrequencyInt;

            ProcessWaveFileWithAudioFunctionsAndStore(strWaveFilePath, new List<DataProcessorAndStore> { dPandS },
                                                       samplingFreq,
                                                       SAMPLES,
                                                       indexOfFile
                                                       );
            // ssave the file
            string shFilename = comFunc.GetFileNameFromPath(strWaveFilePath);

            shFilename = comFunc.GetDcbFileNameForThisFreqFile(strWaveFilePath);
            dpHlp.DiscardDataPointsWhereXlessThan(0.5f);
            dpHlp.SaveDataPointsToFile(shFilename);
        }

        private void ImportFilesToDataFolder(List<ImportFile> lstFilesToImport)
        {

            for (int i = 0; i < lstFilesToImport.Count; i++)
            {
                ImportAudioFileToDataFolder(lstFilesToImport[i].FileName, lstFilesToImport[i].ConvertFrom32to16bit, i);
            }

        }

        private bool ImportAudioFileToDataFolder(string fileName, bool convertFrom32to16bit, int indexOfFile)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            int intCurPercent = 0;
            string shFilenameWithExtension = comFunc.GetFileNameFromPath(fileName);
            string shFilenameDest = comFunc.GetFilePathAndNameWtihoutExtension(shFilenameWithExtension) + CommonFunctions.WAV_FILE_EXTENSION;
            string strDestPath = System.IO.Path.Combine(Common.Settings.DataFolder, shFilenameDest);
            ConvertAudioFileToWave cvAudio;

            // in term of file progress reported
            // the conversion gets 20 % of the progress and the GenerateFrequencyTextFile gets 80%
            try
            {
                cvAudio = new ConvertAudioFileToWave(m_act, Common.Settings.SampleRateInt, 1, fileName, strDestPath);

                cvAudio.CalculateDuration();

                cvAudio.DoConversion();
                while (!cvAudio.Finished)
                {
                    intCurPercent = (int)((cvAudio.ElaspedTimeInSeconds / cvAudio.DurationInSeconds) * 100);
                    FileProgressArgs fp = new FileProgressArgs((intCurPercent * (CONVERT_PROGESS_TOTAL)) / 100, indexOfFile, false);
                    FileProgress?.Invoke(this, fp);
                    Thread.Sleep(500);
                }

                if (cvAudio.ExceptionMessage != null)
                {
                    throw new Exception(cvAudio.ExceptionMessage);
                }
            
                // now generate the freq text file

                GenerateFrequencyTextFile(strDestPath, indexOfFile);
            }
            catch (Exception e)
            {
                throw new Exception("Error Importing " + fileName + " - " + e.Message);
            }
            return true;
        }


        private void ProcessWaveFileWithAudioFunctionsAndStore(string strFilePath, List<DataProcessorAndStore> lstDataGnStore,
                                                                int samplingFreq, int bufferSize, int indexOfFile)
        {
            // this function will also call the fileprogress event handlers (this is why indexoffile was passsed in)
            DataPointHelper<Serializable_DataPoint> dpHlp = DataPointCollection.Frq;
            int sampleRate, samplePeriodBytes;
            int startIndexBytes, indexBytes, intNumChunks, intCurrentPercentTime, intPerctTime = 0;
            Int16 intVal;
            short[] sBuffer = new short[bufferSize];
            long lgLength = Common.FileHelper.GetLengthOfFile(strFilePath, false);
            const int CHUNK_SIZE = 1048576;  // 1MB
            double dbTotalTime, dbTime;
            int bytesPerSecondInAudioFile;
            (_, _, _, sampleRate, bytesPerSecondInAudioFile) = ReadWavParameters(strFilePath);

            samplePeriodBytes = (int)(bytesPerSecondInAudioFile / samplingFreq);

            dbTotalTime = lgLength / (double)bytesPerSecondInAudioFile;

            intNumChunks = 0;
            foreach (byte[] rawData in GetDataChunkFromWaveFile(strFilePath, CHUNK_SIZE))
            {
                startIndexBytes = 0;
                while (startIndexBytes + (sBuffer.Length * 2) + 2 < rawData.Length)
                {
                    indexBytes = startIndexBytes;
                    // fill data buffer
                    for (int i = 0; i < sBuffer.Length; i++)
                    {
                        intVal = BitConverter.ToInt16(rawData, indexBytes += 2);
                        sBuffer[i] = intVal;
                    }
                    dbTime = ((intNumChunks * CHUNK_SIZE) + indexBytes) / (double)bytesPerSecondInAudioFile;
                    //notify listeners of progress
                    intCurrentPercentTime = (int)((dbTime / dbTotalTime) * 100);
                    if (intCurrentPercentTime > intPerctTime + FILE_GENERATION_PROGESS_PERCENT_INCREMENTS)
                    {
                        intPerctTime += FILE_GENERATION_PROGESS_PERCENT_INCREMENTS;
                        FileProgressArgs fp = new FileProgressArgs(CONVERT_PROGESS_TOTAL + (intCurrentPercentTime * FILE_GENERATION_PROGESS_TOTAL) / 100, indexOfFile, false);
                        FileProgress?.Invoke(this, fp);
                    }
                    // got data now process it
                    ProcessDataBuffer(sBuffer, lstDataGnStore, dbTime);
                    startIndexBytes += samplePeriodBytes;
                }
                intNumChunks++;
            }

        }

        private void ProcessDataBuffer(short[] sBuffer, List<DataProcessorAndStore> lstDataGnStore, double time)
        {
            foreach (DataProcessorAndStore dp in lstDataGnStore)
            {
                double? calc = dp._pFuncAudioProcessor(sBuffer);
                if (calc != null)
                {
                    dp._dpHlp.AddDataPointToCollection(time, (double)calc);
                }
            }
        }

        public static System.Collections.Generic.IEnumerable<byte[]> GetDataChunkFromWaveFile(string strFilePath, int intChunkSize)
        {
            DataInputStream input = null;
            Stream si;
            byte[] headerData = new byte[(int)WAVE_HEADER_SIZE_BYTES];
            byte[] rawData;
            long lgLength = Common.FileHelper.GetLengthOfFile(strFilePath, false);
            int totalBytesRead = 0, bytesleftToRead;

            try
            {
                si = FileHelper.OpenFileInputStream(strFilePath, false, MediaStoreHelper.MIMETYPE_WAV);
                input = new DataInputStream(si);

                rawData = new byte[(int)intChunkSize];
                totalBytesRead += input.Read(headerData, 0, WAVE_HEADER_SIZE_BYTES);

                while ((lgLength - totalBytesRead) > 0)
                {
                    bytesleftToRead = (int)lgLength - totalBytesRead;
                    if (bytesleftToRead >= intChunkSize)
                    {
                        totalBytesRead += input.Read(rawData, 0, (int)intChunkSize);
                    }
                    else
                    {
                        //end of the data won't have to resize again after this
                        Array.Resize<byte>(ref rawData, bytesleftToRead);
                        totalBytesRead += input.Read(rawData, 0, (int)bytesleftToRead);
                    }
                    yield return rawData;
                }
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }

        }

        private void GenerateFrequencyTextFile(string strFilePath, int indexOfFile)
        {
            Common.CommonFunctions comFunc = new Common.CommonFunctions();
            DSP.MPM m_dspLib;
            int sampleRate;
            int SAMPLES = Common.Settings.NumberOfSamplesInBuffer;
            DataPointHelper<Serializable_DataPoint> dpHlp = DataPointCollection.Frq;
            long lgLength = Common.FileHelper.GetLengthOfFile(strFilePath, false);

            (_, _, _, sampleRate, _) = ReadWavParameters(strFilePath);

            m_dspLib = new DSP.MPM(sampleRate, SAMPLES, 0.93f);

            DataProcessorAndStore dPandS = new DataProcessorAndStore(
                                           new AudioDataProcessorDelegate(m_dspLib.getPitchFromShort),
                                           dpHlp,
                                           1, "Hertz");

            // find out how many samples we need per sec
            int samplingFreq = samplingFreq = Common.Settings.PitchSamplingFrequencyInt;

            ProcessWaveFileWithAudioFunctionsAndStore(strFilePath, new List<DataProcessorAndStore> { dPandS },
                                                       samplingFreq,
                                                       SAMPLES,
                                                       indexOfFile
                                                       );


            if (dPandS._dpHlp.NumberOfDataPoints < 5)
            {
                Common.FileHelper.DeleteFile(strFilePath);
                throw new Exception("This file does not have any detectable frequency points");
            }

            // save the file           
            string shFilename = comFunc.GetFileNameFromPath(strFilePath);
            shFilename = shFilename.Substring(0, (shFilename.LastIndexOf('.'))) + CommonFunctions.TEXT_EXTENSION;
            dPandS._dpHlp.SaveDataPointsToFile(shFilename);


            if (indexOfFile == m_numFiles - 1)
            {
                //this will close the file progress dialog
                FileProgressArgs filep = new FileProgressArgs(100, indexOfFile, true);
                FileProgress(this, filep);
            }
        }



        private void StereoToMono(byte[] rawData, byte[] newData, short numChannels, int indexOfFile)
        {
            int intNextPerct = 0, intCurPercent;

            switch (numChannels)
            {
                case 1:

                    for (int i = 0; i + 2 < rawData.Length; i += 2)
                    {
                        // nothing to do other than copy
                        newData[i] = rawData[i];
                        newData[i + 1] = rawData[i + 1];
                        intCurPercent = (i * 100) / rawData.Length;
                        if (intCurPercent > intNextPerct + CONVERT_PROGESS_PERCENT_INCREMENTS)
                        {
                            intNextPerct += CONVERT_PROGESS_PERCENT_INCREMENTS;
                            FileProgressArgs fp = new FileProgressArgs(CONVERT_PROGESS_TOTAL / 2 + (intCurPercent * (CONVERT_PROGESS_TOTAL / 2)) / 100, indexOfFile, false);
                            FileProgress?.Invoke(this, fp);
                        }

                    }
                    break;

                case 2:
                    for (int i = 0; i + 4 < rawData.Length; i += 4)
                    {
                        short leftChannel, rightChannel, result;

                        rightChannel = BitConverter.ToInt16(rawData, i);
                        leftChannel = BitConverter.ToInt16(rawData, i + 2);
                        result = (short)(((int)rightChannel + leftChannel) / 2);

                        newData[i / 2] = (byte)(result & 0x00FF);
                        newData[(i / 2) + 1] = (byte)((result >> 8) & 0x00FF);

                        intCurPercent = (i * 100) / rawData.Length;

                        if (intCurPercent > intNextPerct + CONVERT_PROGESS_PERCENT_INCREMENTS)
                        {
                            intNextPerct += CONVERT_PROGESS_PERCENT_INCREMENTS;
                            FileProgressArgs fp = new FileProgressArgs(CONVERT_PROGESS_TOTAL / 2 + (intCurPercent * (CONVERT_PROGESS_TOTAL / 2)) / 100, indexOfFile, false);
                            FileProgress?.Invoke(this, fp);
                        }

                    }
                    break;
            }

        }

        private void ResampleBitDepthToNewData(byte[] rawData, byte[] newData, int bitDepth, int indexOfFile)
        {
            int intNextPerct = 0, intCurPercent;
            switch (bitDepth)
            {
                case 32:

                    for (int i = 0; i + 4 < rawData.Length; i += 4)
                    {
                        // android is little endian (lsB stored first)
                        // take the two most significant bytes

                        newData[i / 2] = rawData[i + 2];
                        newData[(i / 2) + 1] = rawData[i + 3];

                        intCurPercent = (i * 100) / rawData.Length;
                        if (intCurPercent > intNextPerct + CONVERT_PROGESS_PERCENT_INCREMENTS)
                        {
                            intNextPerct += CONVERT_PROGESS_PERCENT_INCREMENTS;
                            FileProgressArgs fp = new FileProgressArgs((intCurPercent * (CONVERT_PROGESS_TOTAL / 2)) / 100, indexOfFile, false);
                            FileProgress?.Invoke(this, fp);
                        }

                    }
                    break;

                case 24:
                    for (int i = 0; i + 3 < rawData.Length; i += 3)
                    {
                        // android is little endian (lsB stored first)
                        // take the two most significant bytes

                        newData[(i * 2) / 3] = rawData[i + 1];
                        newData[((i * 2) / 3) + 1] = rawData[i + 2];

                        // give updates at 10% s

                        intCurPercent = (i * 100) / rawData.Length;
                        if (intCurPercent > intNextPerct + CONVERT_PROGESS_PERCENT_INCREMENTS)
                        {
                            intNextPerct += CONVERT_PROGESS_PERCENT_INCREMENTS;
                            FileProgressArgs fp = new FileProgressArgs((intCurPercent * (CONVERT_PROGESS_TOTAL / 2)) / 100, indexOfFile, false);
                            FileProgress?.Invoke(this, fp);
                        }




                    }
                    break;

                case 16:

                    for (int i = 0; i + 2 < rawData.Length; i += 2)
                    {
                        newData[i] = rawData[i];
                        newData[i + 1] = rawData[i + 1];

                        intCurPercent = (i * 100) / rawData.Length;
                        if (intCurPercent > intNextPerct + CONVERT_PROGESS_PERCENT_INCREMENTS)
                        {
                            intNextPerct += CONVERT_PROGESS_PERCENT_INCREMENTS;
                            FileProgressArgs fp = new FileProgressArgs((intCurPercent * (CONVERT_PROGESS_TOTAL / 2)) / 100, indexOfFile, false);
                            FileProgress?.Invoke(this, fp);
                        }
                    }
                    break;


            }
        }


        // private (int, int) ReadWavParameters(string filename, ref int pBitDepth, ref int pFormatCode, ref Int16 numChannels, ref int sampleRate, ref int bytesPerSec)
        private (int, int, Int16, int, int) ReadWavParameters(string filename)

        {
            //float [] left = new float[1];
            int pBitDepth, pFormatCode;
            Int16 numChannels;
            int sampleRate, bytesPerSec;
            //float [] right;
            try
            {
                using (Stream st = FileHelper.OpenFileInputStream(filename, false, MediaStoreHelper.MIMETYPE_WAV))
                {
                    BinaryReader reader = new BinaryReader(st);

                    // chunk 0
                    int chunkID = reader.ReadInt32();
                    int fileSize = reader.ReadInt32();
                    int riffType = reader.ReadInt32();


                    // chunk 1
                    int fmtID = reader.ReadInt32();
                    int fmtSize = reader.ReadInt32(); // bytes for this chunk
                    int fmtCode = reader.ReadInt16();

                    pFormatCode = fmtCode;


                    Int16 channels = reader.ReadInt16();
                    numChannels = channels;

                    sampleRate = reader.ReadInt32();
                    int byteRate = reader.ReadInt32();
                    int fmtBlockAlign = reader.ReadInt16();
                    int bitDepth = reader.ReadInt16();

                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        int fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    // chunk 2
                    int dataID = reader.ReadInt32();
                    int bytes = reader.ReadInt32();

                    pBitDepth = bitDepth;

                    bytesPerSec = sampleRate * (pBitDepth / 8) * numChannels;


                }
            }
            catch (Exception e1)
            {
                throw e1;
                //left = new float[ 1 ]{ 0f };
            }

            return (pBitDepth, pFormatCode, numChannels, sampleRate, bytesPerSec);
        }
    }


}


