namespace TTtuner_2022_2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;


    using Java.Nio.Channels;
    using Java.IO;
    using System.Threading.Tasks;
    using System.Reflection;
    using TTtuner_2022_2.Plot;
    using TTtuner_2022_2.Common;
    using TTtuner_2022_2.EventHandlersTidy;
    using AndroidX.Fragment.App;
    using global::Android.Widget;
    using global::Android.Views;
    using global::Android.OS;
    using global::Android.Graphics;
    using global::Android.Content;


    /// <summary>
    ///   A ListFragment that will show the files and subdirectories of a given directory.
    /// </summary>
    /// <remarks>
    ///   <para> This was placed into a ListFragment to make this easier to share this functionality with with tablets. </para>
    ///   <para> Note that this is a incomplete example. It lacks things such as the ability to go back up the directory tree, or any special handling of a file when it is selected. </para>
    /// </remarks>
    public class FileListFragment : ListFragment
    {
        internal static readonly string DefaultInitialDirectory = "/";
        private FileListAdapter _adapter;
        private DirectoryInfo _directory;
        TextView _txtFileName;
        ProgressBar prg;
        string[] m_arrString = { "Play", "Rename", "Delete" };

        Item[] items = new Item[]
         {
                new Item("Play", Resource.Drawable.PlayIcon),
                new Item("Rename",  Resource.Drawable.DrawIcon),
                new Item("Delete", Resource.Drawable.TrashIcon),
                new Item("Share", Resource.Drawable.ShareIcon)
         };

        PopUpMenuListAdapter PopMenuItemAdapter;

        // ItemEventArgs 
        string m_strFullFilename;
        View m_viewListItemClicked;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreateFileListAdapter();
        }

        public override void OnActivityCreated(Bundle savedState)
        {
            base.OnActivityCreated(savedState);

            (Activity as FilePickerActivity).FilesChanged += (sender, args) =>
            {
                string str = FileHelper.DataDirectory;
                RefreshFilesList(str);
            };
            (Activity as FilePickerActivity).SaveListViewFromFragment(this);
            ListView.ItemLongClick += OnListItemLongClick;
        }

        internal void CreateFileListAdapter()
        {
            // todo: get context and pass in below
            _adapter = new FileListAdapter(Activity, new FileInfoItem[0]);
            ListAdapter = _adapter;
        }

        internal void OnListItemLongClick(object listview, AdapterView.ItemLongClickEventArgs view)
        {
            string strFileName;

            // todo: fix call below
            var fileInfoItem = _adapter.GetItem(view.Position);

            //strFileName = fileInfoItem.FullName;
            strFileName = fileInfoItem.FullPath;

            if ((view.View as LinearLayout).Background.Alpha < 150)
            {
                // item selected already, now deselect
                (view.View as LinearLayout).SetBackgroundColor(Color.Black);

                (Activity as FilePickerActivity).FileItemDeSelected(listview as ListView, strFileName);
            }
            else
            {
                // select item
                (Activity as FilePickerActivity).FileSelected(listview as ListView, strFileName);
                (view.View as LinearLayout).SetBackgroundColor(Color.Green);
                (view.View as LinearLayout).Background.Alpha = 100;
            }
        }


        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            //todo fix call below
            var fileInfoItem = _adapter.GetItem(position);
            ProgressBar prg;
            View listItemView;
            bool blAtleastOneFileSelected = false;


            for (int i = 0; i < l.Count; i++)
            {
                listItemView = l.GetChildAt(i);

                if (listItemView != null)
                {
                    if ((listItemView as LinearLayout).Background.Alpha < 150)
                    {
                        blAtleastOneFileSelected = true;
                    }

                    (listItemView as LinearLayout).SetBackgroundColor(Color.Black);

                    (listItemView as LinearLayout).Background.Alpha = 255;
                }
            }

             (Activity as FilePickerActivity).DisplayDefaultMenu();

            if (blAtleastOneFileSelected)
            {
                return;
            }

            if (!fileInfoItem.IsDirectory)
            {

                m_strFullFilename = fileInfoItem.FullPath;
                m_viewListItemClicked = v;
                //set alert for executing the task
                AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(v.Context, Resource.Style.MyAlertDialogStyle);
                PlayFile();
            }
            else
            {
                // Dig into this directory, and display it's contents
                RefreshFilesList(fileInfoItem.FullPath);
            }

            base.OnListItemClick(l, v, position, id);
        }

        internal void PlayFile()
        {
            CommonFunctions cmFunc = new CommonFunctions();

            if (cmFunc.GetFileNameExtension(m_strFullFilename) == CommonFunctions.WAV_FILE_EXTENSION)
            {
                prg = m_viewListItemClicked.FindViewById<ProgressBar>(Resource.Id.PrgBar);
                prg.Visibility = ViewStates.Visible;

                string[] stringArr = { m_strFullFilename , Common.Settings.DisplayNotesGraph.ToString(),
                                    Common.Settings.DisplayDecibelGraph.ToString() ,
                                     Common.Settings.GraphOverlay   };
                var SecondActivity = new Intent(this.Activity, typeof(GraphScreenActivity));
                SecondActivity.PutExtra("MyData", stringArr);

                StartActivity(SecondActivity);
            }
            else
            {
                prg = m_viewListItemClicked.FindViewById<ProgressBar>(Resource.Id.PrgBar);
                prg.Visibility = ViewStates.Visible;
                try
                {
                    GC.Collect();
                    Logger.Info(Common.CommonFunctions.APP_NAME, "stats button click -> starting stats act....");
                    string[] strArray = { m_strFullFilename, true.ToString(), false.ToString(), false.ToString(), true.ToString() };
                    Intent _intent = new Intent(this.Activity, new StatsActivity().Class);
                    _intent.PutExtra("MyData", strArray);

                    StartActivity(_intent);
                }
                catch (Exception e1)
                {
                    Logger.Info(Common.CommonFunctions.APP_NAME, "Exception in graphscreen act!!....");
                    throw e1;
                }

            }
            Activity.Finish();
        }

        internal void Destroy()
        {
            ListView.ItemLongClick -= OnListItemLongClick;

            ListView.RemoveAllViews();
            ListView.Dispose();
            ListAdapter.Dispose();
            this.Dispose();
            base.Dispose();
        }



        public override void OnResume()
        {
            base.OnResume();

            foreach (FileListRowViewHolder flRow in _adapter.ListViewHolder)
            {
                flRow.ProgressBar.Visibility = ViewStates.Gone;
            }

            string str = FileHelper.DataDirectory;
            RefreshFilesList(str);
        }

        internal void RefreshFilesList(string directory)       
        {
            var fileList = FileHelper.GetMediaFileListInDirectory(directory);

            if (fileList.Count == 0)
            {
                AndroidX.AppCompat.App.AlertDialog.Builder dlMsg = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                dlMsg.SetMessage("No files Saved");
                dlMsg.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlMsg.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlMsg.Show();
            }

            _adapter.AddDirectoryContents(fileList);
            // If we don't do this, then the ListView will not update itself when then data set 
            // in the adapter changes. It will appear to the user that nothing has happened.
            ListView.RefreshDrawableState();
#if Debug
            Logger.Verbose("FileListFragment", "Displaying the contents of directory {0}.", directory);
#endif
        }
    }
}
