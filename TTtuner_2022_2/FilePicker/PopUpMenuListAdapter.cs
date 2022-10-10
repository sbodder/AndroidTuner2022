namespace TTtuner_2022_2
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::Android.Content;
    using global::Android.Views;
    using global::Android.Widget;
    using System.Reflection;

    internal class Item
    {
        internal readonly string text;
        internal readonly int icon;
        internal Item(string text, int? icon)
        {
            this.text = text;
            this.icon = icon.Value;
        }
        public override string ToString()
        {
            return text;
        }
    }

    internal class PopUpMenuListAdapter : ArrayAdapter<Item>
    {
        private readonly Context _context;
        private Item[] arrItemList;

       
        internal PopUpMenuListAdapter(Context context, Item[] arrItem) : base(context, Resource.Layout.PopUpListItem, Resource.Id.popUp_text1, arrItem)
        {
            _context = context;

            arrItemList = arrItem;
        }

      
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //Use super class to create the View
            View v = base.GetView(position, convertView, parent);

            TextView txt = (TextView)v.FindViewById(Resource.Id.popUp_text1);

            ImageView img = (ImageView)v.FindViewById(Resource.Id.popUp_image1);

            txt.Text = arrItemList[position].text;

            img.SetImageResource(arrItemList[position].icon);

            return v;
        }

    }
}
