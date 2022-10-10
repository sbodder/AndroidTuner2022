using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NUnit.Framework;
using TTtuner_2022_2.Music;
using TTtuner_2022_2.Plot;
using static TTtuner_2022_2.Music.NoteStatsGenerator;

namespace TTtunerUnitTests.Music
{
    [TestFixture]
    class NoteStatTest
    {
        private ObservableCollection<NoteStat> _nsCol1, _nsCol2;
        private ObservableCollection<ISerialisableDataPoint> _points1, _points2;
        NoteStatsGenerator _ng;
        [SetUp]
        public void Setup()
        {
            _ng = new NoteStatsGenerator(0);
            SetupDataPoints();
            
            _nsCol1 = _ng.GenerateStats_Freq(_points1);
            _nsCol2 = _ng.GenerateStats_Freq(_points2);
        }

        private void SetupDataPoints()
        {
            _points1 = new ObservableCollection<ISerialisableDataPoint>();
            _points1.Add(new Serializable_DataPoint(0, 1319.001));
            _points1.Add(new Serializable_DataPoint(1, 1325.001));
            _points1.Add(new Serializable_DataPoint(2, 1313.001));

            _points2 = new ObservableCollection<ISerialisableDataPoint>();
            _points2.Add(new Serializable_DataPoint(4, 1335));
            _points2.Add(new Serializable_DataPoint(5, 1310));
            _points2.Add(new Serializable_DataPoint(2, 1340));
        }

        [TearDown]
        public void Tear() { }

        [TestCase]
        public void MergeTest()
        {
            ObservableCollection<NoteStat> nsRes;
            double dlCentError =0, dbTotalCentsError = 0;
            foreach (Serializable_DataPoint dp in _points1)
            {
                NotePitchMap.GetNoteFromPitch(dp.YVal, ref dlCentError);
                dbTotalCentsError += dlCentError;
            }
            foreach (Serializable_DataPoint dp in _points2)
            {
                NotePitchMap.GetNoteFromPitch(dp.YVal, ref dlCentError);
                dbTotalCentsError += dlCentError;
            }

            _ng.MergeNoteStatCollections(_nsCol1, _nsCol2);

            Assert.True(_nsCol1.Count == 1, $"The count is {_nsCol1.Count}");
            Assert.True(_nsCol1[0].Note == "E6", $"The note is {_nsCol1[0].Note}");
            //Assert.True(_nsCol1[0].Samples == 6);
            //Assert.True(_nsCol1[0].Samples == 6);
            //Assert.True(_nsCol1[0].AverageCentsError == (dbTotalCentsError/ 6).ToString("F2", CultureInfo.InvariantCulture) );
        }
    }
}