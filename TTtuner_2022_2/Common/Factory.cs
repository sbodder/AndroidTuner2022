using Android.App;
using Android.Graphics;
using Com.Syncfusion.Charts;
using Syncfusion.SfDataGrid;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TTtuner_2022_2.Audio;
using TTtuner_2022_2.Music;
using TTtuner_2022_2.Plot;
using TTtuner_2022_2.ControlStyles;
using Android.Content.Res;
using static TTtuner_2022_2.Music.NoteStatsGenerator;
using System.ComponentModel;
using System;
using Syncfusion.Data;
using System.Collections.Specialized;
using TTtuner_2022_2.DataGrid.Cells;

namespace TTtuner_2022_2.Common
{
    internal class Factory
    {

        internal PlotHelper CreatePlotHelper(Activity act, string strTextFilePath, string strWavFilePath, bool blDisplayNotesGraph, bool blDisplayDecibelGraph, string graphOverlay)
        {
            DataPointHelper<Serializable_DataPoint> dtHelperFreq = DataPointCollection.Frq;
            DataPointHelper<Serializable_DataPoint_Std> dtHelperDecibel = DataPointCollection.Dcb;
            IChart sfChartFreq = null, sfChartDecibel = null;
            SfChartOverviewExt sfChartOverview = null;
            IAudioPlayer audPlay;
            CommonFunctions cmFunc = new CommonFunctions();
            List<IChart> lstChartArray = new List<IChart>();
            AudioFileImporter ai = new AudioFileImporter();
            string strDecibelFilepath;
            const int MIN_NUM_POINTS = 5;            

            if (blDisplayNotesGraph)
            {
                dtHelperFreq.LoadDataPointsFromFile(strTextFilePath);
                if (dtHelperFreq.DataPoints.Count > MIN_NUM_POINTS)
                {
                    sfChartFreq = CreateMainPlot_Freq(act, dtHelperFreq.DataPoints);
                    lstChartArray.Add(sfChartFreq);
                }
            }

            if (blDisplayDecibelGraph)
            {
                if (!cmFunc.DoesDcbFileExistForThisFreqFile(strTextFilePath))
                {
                    ai.GenerateDecibelFile(strWavFilePath, 0);
                }
                strDecibelFilepath = cmFunc.GetDcbFileNameForThisFreqFile(strTextFilePath);
                dtHelperDecibel.LoadDataPointsFromFile(strDecibelFilepath);
                sfChartDecibel = CreateMainPlot_Dcb(act, dtHelperDecibel.DataPoints);
                lstChartArray.Add(sfChartDecibel);
            }


            if (graphOverlay == "Notes Graph")
            {
                if (dtHelperFreq.DataPoints.Count > MIN_NUM_POINTS)
                {
                    sfChartOverview = CreatePlotOverview(act, dtHelperFreq.DataPoints, true, Color.White);
                }
            }
            else if (graphOverlay == "Decibel Graph")
            {
                if (dtHelperDecibel.DataPoints.Count > MIN_NUM_POINTS)
                {
                    sfChartOverview = CreatePlotOverview(act, dtHelperDecibel.DataPoints, false, Color.Red);
                }
            }

            audPlay = CreateAudioPlayer(strWavFilePath);
            return new PlotHelper(act,
                                  audPlay,
                                  lstChartArray.ToArray(),
                                  sfChartOverview);
        }

        private SfChartMainExt_Dcb CreateMainPlot_Dcb(Activity act, ObservableCollection<ISerialisableDataPoint> lstPoints)
        {
            FastLineSeries lsDataPoint, lsVerticalPlayLine;
            SfChartMainExt_Dcb chart = new SfChartMainExt_Dcb(act, lstPoints);
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            float trackLength;

            chart.Title.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 13f);
            chart.Title.SetX(4);
            chart.Title.SetTextColor(Color.White);
            chart.SetBackgroundColor(Color.Black);


            //Initializing primary axis
            NumericalAxis primaryAxis = new Com.Syncfusion.Charts.NumericalAxis();
            primaryAxis.Minimum = 0;
            primaryAxis.Title.Text = "Time";
            chart.PrimaryAxis = primaryAxis;
            primaryAxis.Title.TextColor = Color.White;
            primaryAxis.LabelStyle.TextColor = Color.White;


            //Initializing secondary Axis
            NumericalAxis secondaryAxis = new Com.Syncfusion.Charts.NumericalAxis();
            secondaryAxis.LabelStyle.TextColor = Color.White;
            secondaryAxis.Interval = 1;
            secondaryAxis.ShowMinorGridLines = false;
            secondaryAxis.ShowMajorGridLines = false;
            secondaryAxis.LabelStyle.TextSize = 14;
            secondaryAxis.LabelStyle.Typeface = Typeface.DefaultBold;

            CategoryAxis cat = new CategoryAxis();
            chart.SecondaryAxis = secondaryAxis;
            ChartZoomPanBehaviorExt zoomPanBehavior;
            zoomPanBehavior = new ChartZoomPanBehaviorExt();
            zoomPanBehavior.SelectionZoomingEnabled = true;
            zoomPanBehavior.LongPress += chart.GraphLongClick;

            chart.Behaviors.Add(zoomPanBehavior);

            lsVerticalPlayLine = new FastLineSeries();
            lsVerticalPlayLine.Color = Color.Gray;
            lsVerticalPlayLine.XBindingPath = "XVal";
            lsVerticalPlayLine.YBindingPath = "YVal";

            chart.Series.Add(lsVerticalPlayLine);


            lsDataPoint = new FastLineSeries();
            lsDataPoint.Color = Color.Red;
            lsDataPoint.XBindingPath = "XVal";
            lsDataPoint.YBindingPath = "YVal";
            lsDataPoint.ItemsSource = new ObservableCollection<Serializable_DataPoint_Std>();

            foreach (Serializable_DataPoint_Std sd in lstPoints)
            {
                (lsDataPoint.ItemsSource as ObservableCollection<Serializable_DataPoint_Std>).Add(new Serializable_DataPoint_Std(sd.XVal, sd.YVal));
            }

            chart.Series.Add(lsDataPoint);
            chart.Legend.Visibility = Visibility.Gone;

            if (!(bool)Settings.DefaultZoom_Db)
            {
                trackLength = CalculateMaxXval(lstPoints);
                chart.PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Db / 1000f) / trackLength;
                chart.SecondaryAxis.ZoomFactor = Settings.ZoomY_Db;
            }

            return chart;
        }

        private SfChartMainExt_Freq CreateMainPlot_Freq(Activity act, ObservableCollection<ISerialisableDataPoint> lstPoints)
        {
            FastLineSeries lsDataPoint, lsVerticalPlayLine;
            SfChartMainExt_Freq chart = new SfChartMainExt_Freq(act, lstPoints);
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();
            float trackLength;
            chart.Title.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 13f);
            chart.Title.SetX(4);
            chart.Title.SetTextColor(Color.White);
            chart.SetBackgroundColor(Color.Black);


            //Initializing primary axis
            NumericalAxis primaryAxis = new Com.Syncfusion.Charts.NumericalAxis();
            primaryAxis.Minimum = 0;
            primaryAxis.Title.Text = "Time";
            chart.PrimaryAxis = primaryAxis;
            primaryAxis.Title.TextColor = Color.White;
            primaryAxis.LabelStyle.TextColor = Color.White;            


            //Initializing secondary Axis
            NumericalAxisExt secondaryAxis = new NumericalAxisExt();
            secondaryAxis.Maximum = ghFunc.LogValueForGraph_Get(NotePitchMap.PITCH_HIGH_LIMIT);
            secondaryAxis.Minimum = 0;
            secondaryAxis.GetNoteMkInRange = chart.GetNoteMakersInRange;
            secondaryAxis.LabelStyle.TextColor = Color.White;
            secondaryAxis.Interval = 1;
            secondaryAxis.ShowMinorGridLines = false;
            secondaryAxis.ShowMajorGridLines = false;
            secondaryAxis.LabelStyle.TextSize = 14;
            secondaryAxis.LabelStyle.Typeface = Typeface.DefaultBold;

            CategoryAxis cat = new CategoryAxis();

            chart.SecondaryAxis = secondaryAxis;
            ChartZoomPanBehaviorExt zoomPanBehavior;
            zoomPanBehavior = new ChartZoomPanBehaviorExt();
            zoomPanBehavior.SelectionZoomingEnabled = true;
            zoomPanBehavior.LongPress += chart.GraphLongClick;

            chart.Behaviors.Add(zoomPanBehavior);

            lsVerticalPlayLine = new FastLineSeries();
            lsVerticalPlayLine.Color = Color.Gray;
            lsVerticalPlayLine.XBindingPath = "XVal";
            lsVerticalPlayLine.YBindingPath = "YVal";
            chart.Series.Add(lsVerticalPlayLine);


            lsDataPoint = new FastLineSeries();
            lsDataPoint.Color = Color.White;
            lsDataPoint.XBindingPath = "XVal";
            lsDataPoint.YBindingPath = "YVal";
            lsDataPoint.ItemsSource = new ObservableCollection<Serializable_DataPoint>();

            foreach (Serializable_DataPoint sd in lstPoints)
            {
                (lsDataPoint.ItemsSource as ObservableCollection<Serializable_DataPoint>).Add(new Serializable_DataPoint(sd.XVal, ghFunc.LogValueForGraph_Get(sd.YVal), sd.PctCloseness, sd.Note));
            }

            chart.Series.Add(lsDataPoint);
            chart.Legend.Visibility = Visibility.Gone;

            if (!(bool)Settings.DefaultZoom_Freq)
            {
                trackLength = CalculateMaxXval(lstPoints);
                chart.PrimaryAxis.ZoomFactor = (Settings.ZoomXms_Freq / 1000f) / trackLength;
                chart.SecondaryAxis.ZoomFactor = Settings.ZoomY_Freq;
            }

            return chart;
        }


        private SfChartOverviewExt CreatePlotOverview(Activity act, ObservableCollection<ISerialisableDataPoint> lstPoints, bool blLogarithmVals, Color col)
        {
            FastLineSeries lsDataPoint, lsVerticalPlayLine;
            SfChartOverviewExt chart = new SfChartOverviewExt(act, lstPoints);
            ObservableCollection<ISerialisableDataPoint> lstDataPoint;
            GraphHelper_Freq ghFunc = new GraphHelper_Freq();

            chart.Title.SetTextColor(col);
            chart.SetBackgroundColor(Color.Black);


            //Initializing primary axis
            NumericalAxis primaryAxis = new Com.Syncfusion.Charts.NumericalAxis();
            // primaryAxis.Minimum = 0;
            chart.PrimaryAxis = primaryAxis;
            primaryAxis.Title.TextColor = Color.White;
            primaryAxis.LabelStyle.TextColor = Color.White;

            NumericalAxis secondaryAxis = new NumericalAxis();

            secondaryAxis.LabelStyle.TextColor = Color.White;
            //  secondaryAxis.LabelCreated += SecondaryAxis_LabelCreate;
            secondaryAxis.Interval = 1;
            secondaryAxis.ShowMinorGridLines = false;
            secondaryAxis.ShowMajorGridLines = false;
            secondaryAxis.LabelStyle.TextSize = 0.0f;
            secondaryAxis.LabelStyle.Typeface = Typeface.Default;


            // secondaryAxis.

            chart.SecondaryAxis = secondaryAxis;

            ChartZoomPanBehavior zoomPanBehavior = new ChartZoomPanBehavior();
            zoomPanBehavior.SelectionZoomingEnabled = false;

            //zoomPanBehavior.ZoomMode = ZoomMode.Xy;
            chart.Behaviors.Add(zoomPanBehavior);

            lsVerticalPlayLine = new FastLineSeries();
            lsVerticalPlayLine.Color = Color.Green;
            lsVerticalPlayLine.XBindingPath = "XVal";
            lsVerticalPlayLine.YBindingPath = "YVal";



            lsDataPoint = new FastLineSeries();
            lsDataPoint.Color = col;
            lsDataPoint.XBindingPath = "XVal";
            lsDataPoint.YBindingPath = "YVal";
            lsDataPoint.ItemsSource = new ObservableCollection<ISerialisableDataPoint>();
            // lsDataPoint.ItemsSource = m_lstPitchPoints;

            foreach (ISerialisableDataPoint sd in lstPoints)
            {
                (lsDataPoint.ItemsSource as ObservableCollection<ISerialisableDataPoint>).Add(
                                                                        new Serializable_DataPoint_Std(
                                                                        sd.XVal,
                                                                        blLogarithmVals ? ghFunc.LogValueForGraph_Get(sd.YVal) : sd.YVal));
            }

            chart.Series.Add(lsDataPoint);

            // add the vertical line after the data series as it will be displayed on top of it as a result
            chart.Series.Add(lsVerticalPlayLine);


            chart.Legend.Visibility = Visibility.Gone;


            int numPoints = ((chart.Series[SfChartOverviewExt.INDEX_OF_DATA_SERIES_ON_CHART_OVERVIEW] as FastLineSeries).ItemsSource as ObservableCollection<ISerialisableDataPoint>).Count;

            double minY, maxY, minX = 0, maxX;
            lstDataPoint = ((chart.Series[SfChartOverviewExt.INDEX_OF_DATA_SERIES_ON_CHART_OVERVIEW] as FastLineSeries).ItemsSource as ObservableCollection<ISerialisableDataPoint>);
            maxY = chart.GetMaxYVAlueInList(lstDataPoint);
            minY = chart.GetMinYVAlueInList(lstDataPoint);
            maxX = lstDataPoint[lstDataPoint.Count - 1].XVal;

            chart.m_flMaxTimeInDataSeries = (float)maxX;

            (chart.Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(chart.PrimaryAxis, minX, maxX);
            (chart.Behaviors[0] as ChartZoomPanBehavior).ZoomByRange(chart.SecondaryAxis, minY, maxY);

            (chart.Behaviors[0] as ChartZoomPanBehavior).ZoomingEnabled = false;

            chart.Behaviors.Clear();

            chart.m_MinYvalOnChartOverview = minY;
            chart.m_MaxYvalOnChartOverview = maxY;

            return chart;

        }

        internal IAudioPlayer CreateAudioPlayer(string strWavFilePath)
        {
            IAudioPlayer audPlay;

            // if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)

            if (Common.Settings.AudioPlayer == "TimeStretch")
            {
                audPlay = new RubberBand_AudioPlayer();
            }
            else
            {
                audPlay = new AudioTrack_AudioPlayer();
            }

            audPlay.SetupPlayer(strWavFilePath, 1.0f, false);

            return audPlay;
        }

        internal SfDataGrid CreateFreqDataGrid(Activity act, global::Android.Util.DisplayMetrics displayMetrics, int numOctPerGrid, int heightOfView)
        {
            const int NUM_OCTAVES = 9;
            List<SfDataGrid> lstDataGrid = new List<SfDataGrid>();


            return SetupDataGrid_Freq(act, displayMetrics, 0, 8, heightOfView);
        }   

        private SfDataGrid SetupDataGrid_Freq(Activity act, global::Android.Util.DisplayMetrics displayMetrics, int minOct, int maxOct, int heightOfView)
        {
            SfDataGrid dataGrid_Freq = new SfDataGrid(act);

            dataGrid_Freq.AutoGenerateColumns = false;

            dataGrid_Freq.ScrollingMode = ScrollingMode.PixelLine;
            dataGrid_Freq.FrozenColumnsCount = 1;

            addColsFreq(dataGrid_Freq, displayMetrics, 0.9f, minOct, maxOct);
         
            dataGrid_Freq.GridStyle = new DarkGridStyle();

            dataGrid_Freq.HeaderRowHeight =  30;
            dataGrid_Freq.RowHeight = (heightOfView - (dataGrid_Freq.HeaderRowHeight)) / (12 * displayMetrics.Density);            

            AddRowsToFreqGrid(dataGrid_Freq);            

            return dataGrid_Freq;
        }     

        private void AddRowsToFreqGrid(SfDataGrid dataGrid_Freq)
        {
            ObservableCollection<NoteStat> oNotes = new ObservableCollection<NoteStat>();

            oNotes.Add(new NoteStat("C"));
            oNotes.Add(new NoteStat("C#"));
            oNotes.Add(new NoteStat("D"));
            oNotes.Add(new NoteStat("Eb"));
            oNotes.Add(new NoteStat("E"));
            oNotes.Add(new NoteStat("F"));
            oNotes.Add(new NoteStat("F#"));
            oNotes.Add(new NoteStat("G"));
            oNotes.Add(new NoteStat("G#"));
            oNotes.Add(new NoteStat("A"));
            oNotes.Add(new NoteStat("Bb"));
            oNotes.Add(new NoteStat("B"));
            dataGrid_Freq.ItemsSource = oNotes;            
        }

        private void addColsFreq(SfDataGrid sfD, global::Android.Util.DisplayMetrics displayMetrics, float percentWidth, int minOct, int maxOct)
        {
            var metrics = displayMetrics;
            float totalWidth = (int)(percentWidth * metrics.WidthPixels);
            int numOctInView = 3;

            GridTextColumn noteNameCol = new GridTextColumn();
            noteNameCol.MappingName = "Note";
            noteNameCol.HeaderText = "Note";
            
            noteNameCol.Width = (totalWidth * 0.12) / displayMetrics.Density;
            sfD.Columns.Add(noteNameCol);

            for (int i = minOct; i <= maxOct; i++)
            {
                GridImageColumn statusCol = new GridImageColumn();
                statusCol.MappingName = $"Image{i}";
                statusCol.HeaderText = i.ToString();
                statusCol.AllowSorting = false;
                statusCol.Width = (totalWidth * 0.5 / numOctInView) / displayMetrics.Density;
                //statusCol.ColumnSizer = ColumnSizer.;
                statusCol.UserCellType = typeof(GridImageCell);

                GridTextColumn avgErrorCol = new GridTextColumn();
                avgErrorCol.MappingName = $"AverageCentsError{i}";
                avgErrorCol.HeaderText = "";
                avgErrorCol.AllowSorting = true;
                avgErrorCol.Width = (totalWidth * 0.38 / numOctInView) / displayMetrics.Density;
                //avgErrorCol.ColumnSizer = ColumnSizer.SizeToHeader;

                sfD.Columns.Add(statusCol);
                sfD.Columns.Add(avgErrorCol);
            }
           
        }

        private float CalculateMaxXval(ObservableCollection<ISerialisableDataPoint> lstPoints)
        {
            int numPoints;
            double maxXVal;
            numPoints = lstPoints.Count;
            maxXVal = lstPoints[numPoints - 1].XVal;
            return (float)maxXVal;
        }       
    }
}