namespace TTtuner_2022_2
{
    using global::Android.Widget;

    using Java.Lang;
    using System.Reflection;

    /// <summary>
    ///   This class is used to hold references to the views contained in a list row.
    /// </summary>
    /// <remarks>
    ///   This is an optimization so that we don't have to always look up the
    ///   ImageView and the TextView for a given row in the ListView.
    /// </remarks>
    internal class FileListRowViewHolder : Object
    {
        internal FileListRowViewHolder(TextView textView, ImageView imageView, ProgressBar progressBar)
        {
            TextView = textView;
            ImageView = imageView;
            ProgressBar = progressBar;
        }

        internal ImageView ImageView { get; private set; }
        internal TextView TextView { get; private set; }
        internal ProgressBar ProgressBar { get; private set; }

        /// <summary>
        ///   This method will update the TextView and the ImageView that are
        ///   are
        /// </summary>
        /// <param name="fileName"> </param>
        /// <param name="fileImageResourceId"> </param>
       
        internal void Update(string fileName, int fileImageResourceId)
        {
            TextView.Text = fileName;
            ImageView.SetImageResource(fileImageResourceId);
        }
    }
}
