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
using Java.IO;
using System.IO;
using TTtuner_2022_2.Common;
using Android.Media;

namespace TTtuner_2022_2.Audio
{
    internal class ConvertPcmToWave
    {
        private bool InstanceFieldsInitialized = false;

        private void InitializeInstanceFields()
        {
            WAV_RIFF_SIZE = LONGINT + ID_STRING_SIZE;
            WAV_FMT_SIZE = (4 * SMALLINT) + (INTEGER * 2) + LONGINT + ID_STRING_SIZE;
            WAV_DATA_SIZE = ID_STRING_SIZE + LONGINT;
            WAV_HDR_SIZE = WAV_RIFF_SIZE + ID_STRING_SIZE + WAV_FMT_SIZE + WAV_DATA_SIZE;
        }

        private readonly int LONGINT = 4;
        private readonly int SMALLINT = 2;
        private readonly int INTEGER = 4;
        private readonly int ID_STRING_SIZE = 4;
        private int WAV_RIFF_SIZE;
        private int WAV_FMT_SIZE;
        private int WAV_DATA_SIZE;
        private int WAV_HDR_SIZE;
        private readonly short PCM = 1;
        private readonly int SAMPLE_SIZE = 2;
        internal int cursor, nSamples;
        internal sbyte[] outputSbyte;



        internal ConvertPcmToWave(int sampleRate, short nChannels, sbyte[] data, int start, int end)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            CreateDataInOutputBuffer(sampleRate, nChannels, data, start, end);

        }

        internal ConvertPcmToWave(int sampleRate, short nChannels, byte[] rawData, string strWaveFileNameOutput)
        {
            sbyte[] sBytes;

            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }


            sBytes = new sbyte[(int)rawData.Length];

            for (int i = 0; i < rawData.Length; i++)
            {
                sBytes[i] = (sbyte)rawData[i];
            }

            CreateDataInOutputBuffer(sampleRate, nChannels, sBytes, 0, sBytes.Length);

            WriteToFile(strWaveFileNameOutput);

        }


        internal ConvertPcmToWave(int sampleRate, short nChannels, string strPcmFileNameInput, string strWaveFileNameOutput)
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                ConvertPcmToWaveApi29Plus(sampleRate, nChannels, strPcmFileNameInput, strWaveFileNameOutput);
            }
            else
            {
                ConvertPcmToWaveLegacy(sampleRate, nChannels, strPcmFileNameInput, strWaveFileNameOutput);
            }


        }

        private void ConvertPcmToWaveApi29Plus(int sampleRate, short nChannels, string strPcmFileNameInput, string strWaveFileNameOutput)
        {

            byte[] rawData = null;
            sbyte[] sBytes;

            //Java.IO.File fl = new Java.IO.File(strPcmFileNameInput);
            //long lgLength = fl.Length();

            //fl.Dispose();

            System.IO.Stream input = null;



            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            try
            {
                //input = new DataInputStream(new System.IO.FileStream(strPcmFileNameInput, System.IO.FileMode.Open, System.IO.FileAccess.Read));

                //rawData = new byte[(int)lgLength];
                //sBytes = new sbyte[(int)lgLength];
                //input.Read(rawData);

                
                //input = MediaStoreHelper.OpenFileInputStream(strPcmFileNameInput, string.Empty, MediaFormat.MimetypeAudioRaw);

                input = FileHelper.OpenFileInputStream(strPcmFileNameInput);

                if (input == null)
                {
                    return;
                }

                rawData = ReadAllBytes(input);

                sBytes = new sbyte[(int)rawData.Length];


            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }

            // now we have read all the data from the .pcm file - delete the file

            FileHelper.DeleteFile(strPcmFileNameInput);



            // short[] shorts = Array.ConvertAll(rawData, b => (short)b);
            // sBytes = Array.ConvertAll(rawData, b => (sbyte)b);

            for (int i = 0; i < rawData.Length; i++)
            {
                sBytes[i] = (sbyte)rawData[i];
            }

            CreateDataInOutputBuffer(sampleRate, nChannels, sBytes, 0, sBytes.Length);

            WriteToFile(strWaveFileNameOutput);

        }
        private void ConvertPcmToWaveLegacy(int sampleRate, short nChannels, string strPcmFileNameInput, string strWaveFileNameOutput)
        {

            byte[] rawData;
            sbyte[] sBytes;

            Java.IO.File fl = new Java.IO.File(strPcmFileNameInput);
            long lgLength = fl.Length();

            fl.Dispose();

            DataInputStream input = null;


            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            try
            {
                input = new DataInputStream(new System.IO.FileStream(strPcmFileNameInput, System.IO.FileMode.Open, System.IO.FileAccess.Read));

                rawData = new byte[(int)lgLength];
                sBytes = new sbyte[(int)lgLength];
                input.Read(rawData);


            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }

            // now we have read all the data from the .pcm file - delete the file

            fl = new Java.IO.File(strPcmFileNameInput);
            fl.Delete();
            fl.Dispose();



            // short[] shorts = Array.ConvertAll(rawData, b => (short)b);
            // sBytes = Array.ConvertAll(rawData, b => (sbyte)b);

            for (int i = 0; i < rawData.Length; i++)
            {
                sBytes[i] = (sbyte)rawData[i];
            }

            CreateDataInOutputBuffer(sampleRate, nChannels, sBytes, 0, sBytes.Length);

            WriteToFile(strWaveFileNameOutput);

        }




        private byte[] ReadAllBytes(System.IO.Stream instream)
        {
            if (instream is MemoryStream)
                return ((MemoryStream)instream).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void CreateDataInOutputBuffer(int sampleRate, short nChannels, sbyte[] data, int start, int end)
        {
            nSamples = end - start + 1;
            cursor = 0;
            outputSbyte = new sbyte[nSamples + WAV_HDR_SIZE];
            buildHeader(sampleRate, nChannels);
            writeData(data, start, end);
        }




        // ------------------------------------------------------------
        private void buildHeader(int sampleRate, short nChannels)
        {
            write("RIFF");
            write(outputSbyte.Length);
            write("WAVE");
            writeFormat(sampleRate, nChannels);
        }
        // ------------------------------------------------------------
        internal virtual void writeFormat(int sampleRate, short nChannels)
        {
            write("fmt ");
            write(WAV_FMT_SIZE - WAV_DATA_SIZE);
            write(PCM);
            write(nChannels);
            write(sampleRate);
            write(nChannels * sampleRate * SAMPLE_SIZE);
            write((short)(nChannels * SAMPLE_SIZE));
            write((short)16);
        }
        // ------------------------------------------------------------
        internal virtual void writeData(sbyte[] data, int start, int end)
        {
            write("data");
            write(nSamples);
            for (int i = start; i < end; i++)
            {
                write(data[i]);
            }
        }
        // ------------------------------------------------------------
        private void write(sbyte b)
        {
            outputSbyte[cursor++] = b;
        }
        // ------------------------------------------------------------
        private void write(string id)
        {
            if (id.Length != ID_STRING_SIZE)
            {
                // Utils.logError("String " + id + " must have four characters.");
            }
            else
            {
                for (int i = 0; i < ID_STRING_SIZE; ++i)
                {
                    write((sbyte)id[i]);
                }
            }
        }
        // ------------------------------------------------------------
        private void write(int i)
        {
            write(unchecked((sbyte)(i & 0xFF)));
            i >>= 8;
            write(unchecked((sbyte)(i & 0xFF)));
            i >>= 8;
            write(unchecked((sbyte)(i & 0xFF)));
            i >>= 8;
            write(unchecked((sbyte)(i & 0xFF)));
        }
        // ------------------------------------------------------------
        private void write(short i)
        {
            write(unchecked((sbyte)(i & 0xFF)));
            i >>= 8;
            write(unchecked((sbyte)(i & 0xFF)));
        }
        // ------------------------------------------------------------
        internal bool WriteToFile(string strFilePath)
        {
            bool ok = false;
            byte[] outputByte;

            outputByte = Array.ConvertAll(outputSbyte, (a) => (byte)a);

            try
            {
                Java.IO.File path = new Java.IO.File(strFilePath);
                FileOutputStream outFile = new FileOutputStream(path);
                outFile.Write(outputByte, 0, outputSbyte.Length);
                outFile.Close();
                ok = true;
            }
            catch (Java.IO.FileNotFoundException e)
            {
                //    e.printStackTrace();
                ok = false;
            }
            catch (Java.IO.IOException e)
            {
                ok = false;
                //    e.printStackTrace();
            }
            return ok;
        }
    }
}