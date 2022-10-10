using System;
using System.Collections.Generic;
using Com.Syncfusion.Charts;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;
using TTtuner_2022_2.Music;
using Android.Util;

namespace TTtuner_2022_2
{
    internal class YaxisMarker
    {
        internal double yVal;
        internal string label;

        internal YaxisMarker(double y, string strlabel)
        {
            yVal = y;
            label = strlabel;
        }
    }


    public class NumericalAxisExt : NumericalAxis
    {
        public string NoteToHightlight { get; set; }

        internal delegate List<YaxisMarker> GetNoteMakersInRange(double dblVisibleRangeStart, double dblVisibleRangeEnd);

        private GetNoteMakersInRange m_delGetNoteMarkers;

        internal GetNoteMakersInRange GetNoteMkInRange
        {
            set
            {
                m_delGetNoteMarkers = value;
            }
        }


        protected override void GenerateVisibleLabels()
        {
            base.GenerateVisibleLabels();
            VisibleLabels.Clear();

            Common.CommonFunctions cmFunc = new Common.CommonFunctions();


            List<YaxisMarker> lstNtMarker = m_delGetNoteMarkers(VisibleRange.Start, VisibleRange.End);

            foreach (YaxisMarker nm in lstNtMarker)
            {
                if (NoteToHightlight == nm.label)
                {
                    nm.label = "-> " + nm.label + " <-";
                }
                VisibleLabels.Add(new ChartAxisLabel(nm.yVal, nm.label));
            }

        }
    }
}