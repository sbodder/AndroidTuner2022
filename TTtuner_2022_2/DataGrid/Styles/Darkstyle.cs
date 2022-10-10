using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using Syncfusion.SfDataGrid;

namespace TTtuner_2022_2.ControlStyles
{

    public class DarkGridStyle : DataGridStyle
    {
        public DarkGridStyle()
        {
        }    

        public override GridLinesVisibility GetGridLinesVisibility()
        {
            return GridLinesVisibility.Both;
        }

        public override Color GetHeaderBackgroundColor()
        {
            return Color.Argb(255, 15, 15, 15);
        }

        public override Color GetHeaderForegroundColor()
        {
            return Color.Argb(255, 255, 255, 255);
        }

        public override Color GetRecordBackgroundColor()
        {
            return Color.Argb(255, 0, 0, 0);
        }

        public override Color GetRecordForegroundColor()
        {
            return Color.Argb(255, 255, 255, 255);
        }

        public override Color GetSelectionBackgroundColor()
        {
            return Color.Argb(255, 42, 159, 214);
        }

        public override Color GetSelectionForegroundColor()
        {
            return Color.Argb(255, 255, 255, 255);
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return Color.Argb(255, 02, 02, 02);
        }

        public override Color GetCaptionSummaryRowForeGroundColor()
        {
            return Color.Argb(255, 255, 255, 255);
        }

        public override Color GetBorderColor()
        {
            return Color.Argb(255, 81, 83, 82);
        }

        public override Color GetLoadMoreViewBackgroundColor()
        {
            return Color.Argb(255, 242, 242, 242);
        }

        public override Color GetLoadMoreViewForegroundColor()
        {
            return Color.Argb(255, 34, 31, 31);
        }

        public override Color GetAlternatingRowBackgroundColor()
        {
            return Color.Argb(255, 43, 43, 43);
        }
    }
}