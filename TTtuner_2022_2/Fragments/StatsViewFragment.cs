using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using Syncfusion.Data;
using Syncfusion.SfDataGrid;
using TTtuner_2022_2.Music;
using TTtuner_2022_2.Plot;
using Android.Graphics;
using static TTtuner_2022_2.Music.DbStatsGenerator;
using static TTtuner_2022_2.Music.NoteStatsGenerator;
using TTtuner_2022_2.ControlStyles;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Specialized;
using TTtuner_2022_2.Common;
using TTtuner_2022_2.Adapters;
using TTtuner_2022_2.DataGrid.Cells;
using System.Reflection;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;

namespace TTtuner_2022_2.Fragments
{
    class StatsViewFragment : AndroidX.Fragment.App.Fragment
    {
        private SfDataGrid _dataGrid_Freq;
        private SfDataGrid m_dataGrid_Dcb;
        private AppCompatActivity m_act;
        ObservableCollection<NoteStat> m_lstNoteStats = new ObservableCollection<NoteStat>();
        ObservableCollection<NoteStat> m_tempNoteStats;
        ObservableCollection<DbStat> m_lstDbStat = null;
        ObservableCollection<ISerialisableDataPoint> _collectionBuffer = new ObservableCollection<ISerialisableDataPoint>();
        private bool _displayFreqNotes;
        private bool _displayDcb;
        private bool _bDataSourceMoving;
        private View m_view;
        private bool _viewSetup = false;
        private bool m_dataset = false;
        ManualResetEventSlim mre = new ManualResetEventSlim(true);
        private int _heightOfView;
        LinearLayout m_llayFreq;
        LinearLayout m_llayDcb;
        const float PERCENT_WIDTH = 0.9f;
        private int _statsFragPosition;
        private SwipeIndicatorView _swipeIndicatorView;

        public bool SetupComplete { get; set; } = false;

        public StatsViewFragment()
        {

        }
        public static StatsViewFragment NewInstance(AppCompatActivity act, bool displayFreqNotes, bool displayDb, bool bDataSourceMoving)
        {
            var statsFrag = new StatsViewFragment
            {
                Arguments = new Bundle(),
                m_act = act,
                _displayFreqNotes = displayFreqNotes,
                _displayDcb = displayDb,
                _bDataSourceMoving = bDataSourceMoving,
            };

            return statsFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }
            m_view = inflater.Inflate(Resource.Layout.StatsViewFragment, container, false);
            m_view.LayoutChange += OnLayoutChange;

            _swipeIndicatorView = m_view.FindViewById<SwipeIndicatorView>(Resource.Id.tunerSwipeIndicatorView);
            _swipeIndicatorView.RoundSideFacingLeft = false;
            if (!_bDataSourceMoving)
            {
                _swipeIndicatorView.ShowOval = false;
            }

            SetupData();
            SetupDataGrids(m_view);
            Refresh();
            SetupComplete = true;
            return m_view;
        }

        public void OnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            _statsFragPosition = e.Position;
        }

        public void Destroy()
        {
            m_view.LayoutChange -= OnLayoutChange;
            _dataGrid_Freq.ScrollStateChanged -= DataGrid_ScrollStateChanged;
            _dataGrid_Freq.GridLoaded -= StatFreqGridLoaded;
        }

        public void Freeze()
        {
            LinearLayout m_llay = (LinearLayout)m_view.FindViewById(Resource.Id.LinearLayoutForStatGrid);
            _dataGrid_Freq.GridStyle = new LightGridStyle();
        }

        public void UnFreeze()
        {
            LinearLayout m_llay = (LinearLayout)m_view.FindViewById(Resource.Id.LinearLayoutForStatGrid);
            _dataGrid_Freq.GridStyle = new DarkGridStyle();
        }

        private void SetupData()
        {
            DataPointHelper<Serializable_DataPoint> dataHelperFrq = DataPointCollection.Frq;
            DataPointHelper<Serializable_DataPoint_Std> dataHelperDcb = DataPointCollection.Dcb;
            NoteStatsGenerator ntStat = new NoteStatsGenerator(Common.Settings.MinNumberOfSamplesForNote);
            DbStatsGenerator dbStat = new DbStatsGenerator();

            if (!_bDataSourceMoving)
            {
                // if not moving window of stats then must be static data display and we can calculate all stats now...
                m_lstNoteStats = ntStat.GenerateStats_Freq(dataHelperFrq.DataPoints);
                m_lstDbStat = dbStat.GenerateStats_Db(dataHelperDcb.DataPoints);
            }
        }

        internal void AddPointToGrid(Serializable_DataPoint dp)
        {
            mre.Wait();

            _collectionBuffer.Add((Serializable_DataPoint)dp.Clone());

            mre.Set();
        }

        internal void DeleteAllData()
        {
            mre.Wait();

            _collectionBuffer.Clear();
            foreach (NoteStat ns in (_dataGrid_Freq.ItemsSource as ObservableCollection<NoteStat>))
            {
                ns.DeleteData();
            }
            GC.Collect();
            _dataGrid_Freq.Refresh();

            mre.Set();
        }

        public void Refresh()
        {
            NoteStatsGenerator ntStat = new NoteStatsGenerator(Common.Settings.MinNumberOfSamplesForNote);

            if (_displayFreqNotes)
            {
                mre.Wait();

                m_tempNoteStats = ntStat.GenerateStats_Freq(_collectionBuffer);
                _collectionBuffer.Clear();

                if (m_tempNoteStats.Count == 0)
                {
                    return;
                }

                ntStat.MergeNoteStatCollections(_dataGrid_Freq.ItemsSource as ObservableCollection<NoteStat>, m_tempNoteStats);
                mre.Set();
            }
        }

        private void SetupDataGrids(View view)
        {
            Factory fct = new Factory();
            LinearLayout m_llay = (LinearLayout)view.FindViewById(Resource.Id.LinearLayoutForStatGrid);
            _heightOfView = m_llay.Height;

            if (_displayFreqNotes)
            {
                _dataGrid_Freq = fct.CreateFreqDataGrid(m_act, Resources.DisplayMetrics, 9, GetHeightOfFreqGrid());
                m_llayFreq = new LinearLayout(m_act);
                AddFreqDataGrid();
                m_llay.AddView(m_llayFreq);
            }

            if (_displayDcb)
            {
                SetupDataGrid_Dcb();
                m_llayDcb = new LinearLayout(m_act);
                AddDbDataGrid();
                m_llay.AddView(m_llayDcb);
            }
            AttachDataToDataGrids();
        }

        private void AttachDataToDataGrids()
        {
            NoteStatsGenerator ntStat = new NoteStatsGenerator(Common.Settings.MinNumberOfSamplesForNote);
            if (!_bDataSourceMoving)
            {
                ntStat.MergeNoteStatCollections(_dataGrid_Freq.ItemsSource as ObservableCollection<NoteStat>, m_lstNoteStats);
                if (_displayDcb)
                {
                    m_dataGrid_Dcb.ItemsSource = m_lstDbStat;
                }
            }
        }

        private bool Scrolled;
        private void DataGrid_ScrollStateChanged(object sender, ScrollStateChangedEventArgs e)
        {
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, $"In DataGrid_ScrollStateChanged e.ScrollState : {e.ScrollState} ");
#endif
            if (e.ScrollState == Syncfusion.SfDataGrid.ScrollState.Idle)
            {

                if (Scrolled)
                {
                    Scrolled = false;
#if Release_LogOutput
                    Logger.Info(Common.CommonFunctions.APP_NAME, $"setting e.ScrollState to false : {e.ScrollState} ");
#endif
                    CheckAndScroll();
                }
            }
            else if (e.ScrollState != Syncfusion.SfDataGrid.ScrollState.Programmatic)
            {
                Scrolled = true;
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, $"setting e.ScrollState to true : {e.ScrollState} ");
#endif
            }

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, $"In DataGrid_ScrollStateChanged Scrolled : {Scrolled} ");
#endif

        }

        private void CheckAndScroll()
        {
            try
            {
                var headerRow = _dataGrid_Freq.GetRowGenerator().Items.FirstOrDefault(x => x.RowIndex == _dataGrid_Freq.GetHeaderIndex());
                List<DataColumnBase> visibleColumns = headerRow.GetType().GetProperty("VisibleColumns", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(headerRow) as List<DataColumnBase>;
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll - visibleColumns[0].GridColumn.MappingName : {visibleColumns[0].GridColumn.MappingName } ");
                Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll - visibleColumns[1].GridColumn.MappingName this one!!! : {visibleColumns[1].GridColumn.MappingName } ");
                Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll - visibleColumns[2].GridColumn.MappingName : {visibleColumns[2].GridColumn.MappingName } ");
                Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll - visibleColumns[3].GridColumn.MappingName : {visibleColumns[3].GridColumn.MappingName } ");
#endif
                Settings.LastColumnBrowsedOnStatsGrid = visibleColumns[1].GridColumn.MappingName;

                //                if (visibleColumns[1].GridColumn.MappingName.Contains("Image"))
                //                {
                //                    Settings.LastColumnBrowsedOnStatsGrid = visibleColumns[1].GridColumn.MappingName;
                //                    // _dataGrid_Freq.ScrollToColumnIndex(visibleColumns[1].ColumnIndex);
                //#if Release_LogOutput
                //                    Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll if loop- visibleColumns[1].GridColumn.MappingName: {visibleColumns[1].GridColumn.MappingName } ");
                //#endif
                //                }
                //                else
                //                {
                //                    DataColumnBase dataColumn = visibleColumns.OrderBy(x => x.ColumnIndex).First(x => x.GridColumn.MappingName.Contains("Image"));
                //#if Release_LogOutput
                //                    Logger.Info(Common.CommonFunctions.APP_NAME, $"In CheckAndScroll else loop- dataColumn.GridColumn.MappingName : {dataColumn.GridColumn.MappingName } ");
                //#endif
                //                    Settings.LastColumnBrowsedOnStatsGrid = dataColumn.GridColumn.MappingName;
                //                    _dataGrid_Freq.ScrollToColumnIndex(dataColumn.ColumnIndex);
                //                }
            }
            catch (Exception ex)
            {

            }
        }

        private void AddDbDataGrid()
        {
            var metrics = Resources.DisplayMetrics;
            int heightOfDb = _heightOfView / 5;
            m_llayDcb.AddView(m_dataGrid_Dcb, (int)(metrics.WidthPixels * PERCENT_WIDTH), heightOfDb);
        }

        private int GetHeightOfFreqGrid()
        {
            return _displayDcb ? _heightOfView * 4 / 5 : _heightOfView;
        }

        private void AddFreqDataGrid()
        {
            var metrics = Resources.DisplayMetrics;
            int _heightOfFreq = GetHeightOfFreqGrid();
            _dataGrid_Freq.RowHeight = (_heightOfFreq - (_dataGrid_Freq.HeaderRowHeight)) / (12 * metrics.Density);
            m_llayFreq.AddView(_dataGrid_Freq, (int)(metrics.WidthPixels * PERCENT_WIDTH), _heightOfFreq);
            _dataGrid_Freq.ScrollStateChanged += DataGrid_ScrollStateChanged;
            _dataGrid_Freq.GridLoaded += StatFreqGridLoaded;
            if (_displayDcb)
            {
                var linearLayoutParams = new LinearLayout.LayoutParams((int)(metrics.WidthPixels * PERCENT_WIDTH),
                                                          GetHeightOfFreqGrid());
                linearLayoutParams.SetMargins(0, 0, 0, 30);
                m_llayFreq.LayoutParameters = linearLayoutParams;
            }
        }


        private void addColsDb(SfDataGrid sfD)
        {
            var metrics = Resources.DisplayMetrics;
            float totalWidth = (int)(PERCENT_WIDTH * metrics.WidthPixels);

            GridNumericColumn MeanCol = new GridNumericColumn();
            MeanCol.MappingName = "Mean";
            MeanCol.HeaderText = "X̄(dB)";
            MeanCol.AllowSorting = true;
            MeanCol.NumberDecimalDigits = 0;
            MeanCol.Width = (totalWidth * 0.2) / Resources.DisplayMetrics.Density;

            GridNumericColumn samplesCol = new GridNumericColumn();
            samplesCol.MappingName = "Samples";
            samplesCol.HeaderText = "Samples";
            samplesCol.AllowSorting = true;
            samplesCol.NumberDecimalDigits = 0;
            samplesCol.Width = (totalWidth * 0.2) / Resources.DisplayMetrics.Density;

            GridNumericColumn maxCol = new GridNumericColumn();
            maxCol.MappingName = "Percentile95";
            maxCol.HeaderText = "95%(dB)";
            maxCol.AllowSorting = true;
            maxCol.NumberDecimalDigits = 0;
            maxCol.Width = (totalWidth * 0.2) / Resources.DisplayMetrics.Density;

            GridImageColumn statusCol = new GridImageColumn();
            statusCol.MappingName = "Image";
            statusCol.HeaderText = "";
            statusCol.AllowSorting = false;
            statusCol.Width = (totalWidth * 0.4) / Resources.DisplayMetrics.Density;

            sfD.Columns.Add(MeanCol);
            sfD.Columns.Add(samplesCol);
            sfD.Columns.Add(maxCol);
            sfD.Columns.Add(statusCol);

        }

        private void SetupDataGrid_Dcb()
        {

            m_dataGrid_Dcb = new SfDataGrid(m_act);

            m_dataGrid_Dcb.AutoGenerateColumns = false;

            m_dataGrid_Dcb.ColumnSizer = ColumnSizer.Auto;

            addColsDb(m_dataGrid_Dcb);

            m_dataGrid_Dcb.ScrollingMode = ScrollingMode.Line;

            m_dataGrid_Dcb.HorizontalScrollBarEnabled = false;

            m_dataGrid_Dcb.AllowSorting = true;

            m_dataGrid_Dcb.GridStyle = new DarkGridStyle();

            m_dataGrid_Dcb.HeaderRowHeight = 30;
        }

        public void StatFreqGridLoaded(object sender, EventArgs e)
        {
            try
            {
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, $"In StatFreqGridLoaded - Settings.LastColumnBrowsedOnStatsGrid : {Settings.LastColumnBrowsedOnStatsGrid} ");
#endif
                var columnIndex = _dataGrid_Freq.Columns.IndexOf(_dataGrid_Freq.Columns.FirstOrDefault(x => x.MappingName == Settings.LastColumnBrowsedOnStatsGrid));
                _dataGrid_Freq.ScrollToColumnIndex(columnIndex);
            }
            catch (Exception ex)
            {

            }
        }

        public void OnLayoutChange(object sender, EventArgs e)
        {
            LinearLayout m_llay = (LinearLayout)m_view.FindViewById(Resource.Id.LinearLayoutForStatGrid);
            if (m_llay.Height != _heightOfView)
            {
                _heightOfView = m_llay.Height;

                if (_displayFreqNotes)
                {
                    m_llayFreq.RemoveAllViews();
                    _dataGrid_Freq.ScrollStateChanged -= DataGrid_ScrollStateChanged;
                    _dataGrid_Freq.GridLoaded -= StatFreqGridLoaded;
                    AddFreqDataGrid();
                }

                if (_displayDcb)
                {
                    m_llayDcb.RemoveAllViews();
                    AddDbDataGrid();
                }
            }
        }
    }
}