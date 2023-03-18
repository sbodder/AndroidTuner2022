using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using Syncfusion.SfDataGrid;
using global::Android.Views;
using global::Android.Widget;
using TTtuner_2022_2.Plot;
using System.Collections.ObjectModel;
using TTtuner_2022_2.Music;
using Syncfusion.Data;
using Android.Util;
using Android.Graphics;
using static TTtuner_2022_2.Music.NoteStatsGenerator;
using System.ComponentModel;
using TTtuner_2022_2.Common;
using static TTtuner_2022_2.Music.DbStatsGenerator;
using TTtuner_2022_2.Fragments;
using AndroidX.AppCompat.App;


namespace TTtuner_2022_2
{
    [Activity(Label = "StatsActivity")]
    public class StatsActivity : AppCompatActivity
    {
        private string m_strTextFilePath;
        SfDataGrid m_dataGrid_Freq;
        SfDataGrid m_dataGrid_Dcb;
        Button buttonOk;
        
        ObservableCollection<DbStat> m_lstDbStat = null;
        bool _displayNotes = false, _displayDcb = false;
        StatsViewFragment _statsFrag;
        private bool _displayAsModal;
        private bool _loadDataFromFile;
        private ProgressBar m_prgBAr;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            string[] strArray;

            try
            {
                Logger.Info(Common.CommonFunctions.APP_NAME, "In OnCreate in stats activity ");

                base.OnCreate(savedInstanceState);
                strArray = Intent.GetStringArrayExtra("MyData");
                m_strTextFilePath = strArray[0];
                _displayNotes = strArray[1] == "True" ? true : false;
                _displayDcb = strArray[2] == "True" ? true : false;
                _displayAsModal = strArray[3] == "True" ? true : false;
                _loadDataFromFile = strArray[4] == "True" ? true : false;

                SetContentView(Resource.Layout.StatsScreen);

                SetupToolbar();
                LoadData();
                SetupButton();

                m_prgBAr = FindViewById<ProgressBar>(Resource.Id.PrgBar);
                m_prgBAr.Visibility = ViewStates.Gone;
                _statsFrag = StatsViewFragment.NewInstance( _displayNotes, _displayDcb, false);
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame1, _statsFrag).Commit();

            }
            catch (Exception e)
            {
                Logger.Info(Common.CommonFunctions.APP_NAME, "Exception stats activity ");
                throw e;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            m_prgBAr.Visibility = ViewStates.Gone;
        }

        private void SetupButton()
        {
            buttonOk = (Button)FindViewById(Resource.Id.OkButton1);
            if (!_displayAsModal)
            {
                buttonOk.Visibility = ViewStates.Gone;
            }
            else
            {
                buttonOk.Click += (sender, args) =>
                {
                    Finish();
#if Release_LogOutput
                Logger.Info(Common.CommonFunctions.APP_NAME, "In StatsActivity : Ok button pressed  ");
#endif
            };
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_record1)
            {
                StartActivity(typeof(MainActivity));
                Finish();
            }
            if (item.ItemId == Resource.Id.menu_open1)
            {
                var FilePickerAct = new Intent(this, typeof(FilePickerActivity));
                FilePickerAct.PutExtra("MyData", "MainActivity");
                StartActivity(typeof(FilePickerActivity));
                m_prgBAr.Visibility = ViewStates.Visible;
                Finish();
            }

            if (item.ItemId == Resource.Id.menu_settings1)
            {
                //open the default file
                StartActivity(typeof(SettingsActivity));

                m_prgBAr.Visibility = ViewStates.Visible;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (!_displayAsModal)
            {
#if Release_LogOutput
             MenuInflater.Inflate(Resource.Menu.top_menus_log_output, menu);
#else

                MenuInflater.Inflate(Resource.Menu.StatsMenu, menu);
#endif
            }

            return base.OnCreateOptionsMenu(menu);
        }

        private void LoadData()
        {
            CommonFunctions cmFunc = new CommonFunctions();
            NoteStatsGenerator ntStat = new NoteStatsGenerator(Common.Settings.MinNumberOfSamplesForNote);
            DbStatsGenerator dbStat = new DbStatsGenerator();
            DataPointHelper<Serializable_DataPoint> dataHelperFrq = DataPointCollection.Frq;
            DataPointHelper<Serializable_DataPoint_Std> dataHelperDcb = DataPointCollection.Dcb;

            if (_loadDataFromFile)
            {
                dataHelperFrq.LoadDataPointsFromFile(m_strTextFilePath);
            }
        }

        private void SetupToolbar()
        {
            CommonFunctions cmFunc = new CommonFunctions();

            var toolbar = FindViewById< AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar1);
            if (!_displayAsModal)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.Title = "TTtuner   " + cmFunc.TruncateStringLeft(m_strTextFilePath, 15);
            }
            else
            {
                toolbar.Visibility = ViewStates.Gone;
            }
        }

        public override void OnBackPressed()
        {
            if (!_displayAsModal)
            {
                StartActivity(typeof(MainActivity));
            }
            Finish();

#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In OnBackPressed in Stats Activity  ");
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Info(Common.CommonFunctions.APP_NAME, "in OnDestroy stats act....");
#if Release_LogOutput
            Logger.Info(Common.CommonFunctions.APP_NAME, "In OnDestroy in Stats Activity  ");
#endif
        }
    }
}