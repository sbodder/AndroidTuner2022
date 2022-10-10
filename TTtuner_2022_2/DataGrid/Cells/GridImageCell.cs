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

namespace TTtuner_2022_2.DataGrid.Cells
{
    public class GridImageCell : GridCell
    {
        private ImageView customerImage;
        Bitmap bitmap;

        public GridImageCell(Context context) : base(context)
        {
            customerImage = new ImageView(context);
            this.CanRenderUnLoad = false;
            this.AddView(customerImage);
        }

        protected override void UnLoad()
        {
            if (this.Parent != null)
                (this.Parent as VirtualizingCellsControl).RemoveView(this);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            customerImage.Layout(0, 0, this.Width, this.Height);
            this.SetPadding(0, 0, 0, 0);
        }
        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            bitmap = (Bitmap)DataColumn.CellValue;
            customerImage.SetImageBitmap(bitmap);
        }

        protected override void Dispose(bool disposing)
        {
            if (bitmap != null)
            {
                this.customerImage.Dispose();
                this.customerImage = null;
            }
            base.Dispose(disposing);
        }
    }
}