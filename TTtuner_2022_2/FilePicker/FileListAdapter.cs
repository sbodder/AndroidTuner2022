namespace TTtuner_2022_2
{
    using global::Android.Widget;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using System.Reflection;
    using TTtuner_2022_2.Common;

    internal class FileListAdapter : global::Android.Widget.ArrayAdapter<FileInfoItem>
    {
        private readonly global::Android.Content.Context _context;

        internal List<FileListRowViewHolder> ListViewHolder { get; set; }

        internal FileListAdapter(global::Android.Content.Context context, IList<FileInfoItem> fsi)
            : base(context, Resource.Layout.FilePickerListItem, global::Android.Resource.Id.Text1, fsi)
        {
            _context = context;

            ListViewHolder = new List<FileListRowViewHolder>();
        }

        /// <summary>
        ///   We provide this method to get around some of the
        /// </summary>
        /// <param name="directoryContents"> </param>

   
        internal void AddDirectoryContents(IEnumerable<FileInfoItem> directoryContents)
        {
            Clear();
            // Notify the _adapter that things have changed or that there is nothing 
            // to display.
            if (directoryContents.Any())
            {
#if __ANDROID_11__
                // .AddAll was only introduced in API level 11 (Android 3.0). 
                // If the "Minimum Android to Target" is set to Android 3.0 or 
                // higher, then this code will be used.
                AddAll(directoryContents.ToArray());
#else
                // This is the code to use if the "Minimum Android to Target" is
                // set to a pre-Android 3.0 API (i.e. Android 2.3.3 or lower).
                lock (this)
                    foreach (var fsi in directoryContents)
                    {
                        Add(fsi);
                    }
#endif

                NotifyDataSetChanged();
            }
            else
            {
                NotifyDataSetInvalidated();
            }
        }
       
        public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, global::Android.Views.ViewGroup parent)
        {
            var fileInfoItem = GetItem(position);
            FileListRowViewHolder viewHolder;
            CommonFunctions comFun = new CommonFunctions();
            int resourceIdOfFileIcon;
            global::Android.Views.View row = null;

            if (convertView == null)
            {
                //todo: fix below
                row = _context.GetLayoutInflater().Inflate(Resource.Layout.FilePickerListItem, parent, false);
                viewHolder = new FileListRowViewHolder(row.FindViewById<TextView>(Resource.Id.file_picker_text2), row.FindViewById<ImageView>(Resource.Id.file_picker_image2), row.FindViewById<ProgressBar>(Resource.Id.PrgBar));
                viewHolder.ProgressBar.Visibility = global::Android.Views.ViewStates.Gone;

                row.Tag = viewHolder;

                ListViewHolder.Add(viewHolder);
            }
            else
            {
                row = convertView;
                viewHolder = (FileListRowViewHolder)row.Tag;
            }

            if (fileInfoItem.IsDirectory)
            {
                resourceIdOfFileIcon = Resource.Drawable.folder;
            }
            else if (comFun.GetFileNameExtension(fileInfoItem.Name) == "WAV")
            {
                resourceIdOfFileIcon = Resource.Drawable.file;
            }
            else
            {
                resourceIdOfFileIcon = Resource.Drawable.fileStat;
            }

            viewHolder.Update(fileInfoItem.Name, resourceIdOfFileIcon);

            return row;

        }
    }
}
