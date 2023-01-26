namespace TTtuner_2022_2
{

    using Java.IO;
    using Java.Nio.Channels;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using TTtuner_2022_2.Common;
    using TTtuner_2022_2.EventHandlersTidy;
    using TTtuner_2022_2.Plot;
    using Com.Github.Angads25.Filepicker;
    using Com.Github.Angads25.Filepicker.View;
    using Com.Github.Angads25.Filepicker.Model;
    using Com.Github.Angads25.Filepicker.Controller;
    using TTtuner_2022_2.Audio;
    using Xamarin.Forms.Internals;
    using TTtuner_2022_2.PopUpDialogFragments;
    using System.Threading;
    using TTtuner_2022_2.Music;
    using System.Collections.ObjectModel;
    using BE.Tarsos.Dsp.IO.Android;
    using global::Android.App;
    using AndroidX.AppCompat.App;
    using global::Android.OS;
    using global::Android.Widget;
    using global::Android.Views;
    using global::Android.Content;
    using global::Android.Graphics;
    using Xamarin.Forms.Platform.Android;

    //using System.IO;

    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/ic_launcher")]
    internal class FilePickerActivity : AppCompatActivity
    {
        //private string m_strLastActivity = null;

        internal class FileSelectionListener : Java.Lang.Object, IDialogSelectionListener, global::Android.Runtime.IJavaObject, IDisposable
        {
            internal AndroidX.Fragment.App.FragmentActivity act;
            public void OnSelectedFilePaths(string[] p0)
            {
                AudioFileImporter wvImport = new AudioFileImporter();

                var dialog = PopUpFileProgressDialog.NewInstance("Import File Progress", "Current File :", "Total :"
                                                                , p0.Length, wvImport, (act as FilePickerActivity).UpdateFileList, act);

                dialog.Show(act.GetFragmentManager(), "dialog");
                
                Thread importThread = null;
                importThread = new Thread(() =>
                {
                    Looper.Prepare();
                    wvImport.ImportWaveFiles(act, p0, Common.Settings.DataFolder);
                });
                importThread.Start();
            }
        }

        TextView textDataFolder;
        FileListFragment m_frag;
        FrameLayout framelayoutFiles;
        LinearLayout m_llFiles;
        private IMenu menu;
        private int m_intNumFileMenuItems = 0;
        private int m_intNumMultipleFileMenuItems = 0;
        // private int m_intNumFilesSelected = 0;
        private List<string> m_lstFilesSelected = new List<string>();

        internal event EventHandler<EventArgs> FilesChanged;
        private FilePickerDialog m_FileDialog;
        internal event EventHandler<EventArgs> FilesUpload;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilePickerMain);

            textDataFolder = FindViewById<TextView>(Resource.Id.textDataFolderDisplay);
            framelayoutFiles = FindViewById<FrameLayout>(Resource.Id.framelayoutFiles);

            textDataFolder.Text = FileHelper.DataDirectory;

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar3);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "TTtuner";

            m_llFiles = FindViewById<LinearLayout>(Resource.Id.llFiles);
            framelayoutFiles.Click += (sender, args) =>
            {
                DisplayDefaultMenu();
            };

            //this sets up the taros dsp ffmeg binaries
            AndroidFFMPEGLocator affmeg = new AndroidFFMPEGLocator(this);
            ///
            this.Window.AddFlags(WindowManagerFlags.NotTouchModal);
            // ...but notify us that it happened.

              this.Window.AddFlags(WindowManagerFlags.WatchOutsideTouch);
            FilesUpload += OnUpload;

        }

 

        internal void UpdateFileList()
        {

            RunOnUiThread(() =>
            {
                FilesChanged(this, new EventArgs());
            });
        }


        internal async void OnUpload(object sender, EventArgs e)
        {
            FileSelectionListener lis1 = new FileSelectionListener();
            // string[] strExt = { "wav","mp3", "m4a" };
            string[] strExt = {  };
            File ExternalDir = new File(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);

            lis1.act = this;

            try
            {
                DialogProperties properties = new DialogProperties();
                m_FileDialog = new FilePickerDialog(this, properties);

                m_FileDialog.SetTitle("Select Audio Files");
                m_FileDialog.SetPositiveBtnName("Select");
                m_FileDialog.SetNegativeBtnName("Cancel");

                properties.SelectionMode = DialogConfigs.MultiMode;

                properties.SelectionType = DialogConfigs.FileSelect;

               // properties.Extensions = strExt;

                properties.Root = ExternalDir;

                m_FileDialog.Properties = properties;

                m_FileDialog.SetDialogSelectionListener(lis1);

                m_FileDialog.SetCanceledOnTouchOutside(false);
                m_FileDialog.Show();

            }
            catch (Exception ex)
            {
                AndroidX.AppCompat.App.AlertDialog.Builder dlErrorMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                dlErrorMessage.SetTitle("Error");
                dlErrorMessage.SetMessage("Could not imports files. EXception :" + ex.Message);
                dlErrorMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlErrorMessage.Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.MulitpleFileMenu, menu);

            m_intNumMultipleFileMenuItems = menu.Size();
            for (int i = 0; i < menu.Size(); i++)
            {
                menu.GetItem(i).SetVisible(false);
            }

            MenuInflater.Inflate(Resource.Menu.FileMenu, menu);

            m_intNumFileMenuItems = menu.Size() - m_intNumMultipleFileMenuItems;
            for (int i = m_intNumMultipleFileMenuItems; i < menu.Size(); i++)
            {
                menu.GetItem(i).SetVisible(false);
            }

            MenuInflater.Inflate(Resource.Menu.ArchiveMenu, menu);

            this.menu = menu;
            return base.OnCreateOptionsMenu(menu);
        }

        private void DeleteFiles()
        {
            string str;
            CommonFunctions comFun = new CommonFunctions();

            foreach (string strFilename in m_lstFilesSelected)
            {
                try
                {

                    // delete the wav file
                    Java.IO.File fl = new Java.IO.File(strFilename);
                    if (fl.Exists())
                    {
                        fl.Delete();
                    }

                    fl.Dispose();

                    // delete the data file

                    fl = new Java.IO.File(strFilename.Substring(0, strFilename.Length - 4) + ".TXT");
                    if (fl.Exists())
                    {
                        fl.Delete();
                    }

                    fl.Dispose();

                    //delete dcb file

                    if (comFun.DoesDcbFileExistForThisFreqFile(strFilename))
                    {
                        fl = new Java.IO.File(comFun.GetDcbFileNameForThisFreqFile(strFilename));
                        if (fl.Exists())
                        {
                            fl.Delete();
                        }
                    }


                }
                catch (Exception e1)
                {
                    AndroidX.AppCompat.App.AlertDialog.Builder dlErrorMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                    dlErrorMessage.SetTitle("Error");
                    dlErrorMessage.SetMessage("Could not delete file. EXception :" + e1.Message);
                    dlErrorMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                    dlErrorMessage.Show();

                }
            }

            // refresh the file list    
            FilesChanged(this, new EventArgs());

            DisplayDefaultMenu();

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_settings1)
            {
                StartActivity(typeof(SettingsActivity));
            }

            if (item.ItemId == Resource.Id.menu_record1)
            {
                StartActivity(typeof(MainActivity));
                //kill this activity
                FinishActvity();
            }


            if (item.ItemId == Resource.Id.menu_single_share || item.ItemId == Resource.Id.menu_multiple_share)
            {

                ShareFiles();

            }

            if (item.ItemId == Resource.Id.menu_single_delete || item.ItemId == Resource.Id.menu_multiple_delete)
            {

                DeleteFiles();
            }

            if (item.ItemId == Resource.Id.menu_rename)
            {

                RenameFiles();
            }

            if (item.ItemId == Resource.Id.menu_play)
            {

                PlayFile();
            }

            if (item.ItemId == Resource.Id.menu_import)
            {

#if !Trial_Version
                FilesUpload(this, new EventArgs());
#else
                AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                dlMessage.SetTitle(" Import Files Feature not enabled");
                dlMessage.SetMessage("The Import Files feature is not enabled in the trial version of TTtuner. The full version can be purchased in the Google Play Store.");
                dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });
                dlMessage.Show();
#endif


                // the dataarray of the file will be found in filedata.DataArray 
                // file name will be found in filedata.FileName;
                //etc etc.

            }

            if (item.ItemId == Resource.Id.menu_SaveLog)
            {
                Logger.FlushBufferToFile();
            }


            return base.OnOptionsItemSelected(item);
        }

        private void PlayFile()
        {
            CommonFunctions cmFunc = new CommonFunctions();


            if (cmFunc.GetFileNameExtension(m_lstFilesSelected[0]) == "WAV")
            {
                string[] stringArr = { m_lstFilesSelected[0] , Common.Settings.DisplayNotesGraph.ToString(),
                                    Common.Settings.DisplayDecibelGraph.ToString() ,
                                     Common.Settings.GraphOverlay   };
                var SecondActivity = new Intent(this, typeof(GraphScreenActivity));
                SecondActivity.PutExtra("MyData", stringArr);
                StartActivity(SecondActivity);

            }
            else
            {
                try
                {
                    GC.Collect();
                    Logger.Info(Common.CommonFunctions.APP_NAME, "stats button click -> starting stats act....");
                    string[] strArray = { m_lstFilesSelected[0], true.ToString(), false.ToString(), false.ToString(), true.ToString() };
                    Intent _intent = new Intent(this, new StatsActivity().Class);
                    _intent.PutExtra("MyData", strArray);

                    StartActivity(_intent);
                }
                catch (Exception e1)
                {
                    Logger.Info(Common.CommonFunctions.APP_NAME, "Exception in graphscreen act!!....");
                    throw e1;
                }
            }




            FinishActvity();

        }

        private void RenameFiles()
        {

            EditText editTextView = new EditText(this);
            string strCurrentFileName;
            CommonFunctions comFun = new CommonFunctions();
            string strFileName = m_lstFilesSelected[0];
            string extenstion = comFun.GetFileNameExtension(strFileName);
            strCurrentFileName = strFileName.Substring(strFileName.LastIndexOf('/') + 1, strFileName.LastIndexOf('.') - 1 - strFileName.LastIndexOf('/'));

            AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle("Please Enter New Name");

            editTextView.Text = strCurrentFileName;

            alert.SetView(editTextView); // <----

            alert.SetPositiveButton("OK", (senderAlert, argus) =>
            {
                string str;
                Java.IO.File fl1 = new Java.IO.File(strFileName);
                string txtFilename, strNewDataFileName, strNewWaveFileName, strNewSttFilename;

                try
                {
                    //santiy check the new name
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                    {
                        editTextView.Text = editTextView.Text.Replace(c, '_');
                    }

                    if (extenstion == "STT")
                    {
                        // rename stt file 
                        strNewSttFilename = strFileName.Substring(0, strFileName.LastIndexOf('/') + 1) + editTextView.Text + ".STT";
                        comFun.RenameFile(strFileName, strNewSttFilename);
                    }
                    else
                    {
                        // rename wav file 
                        strNewWaveFileName = strFileName.Substring(0, strFileName.LastIndexOf('/') + 1) + editTextView.Text + ".WAV";
                        comFun.RenameFile(strFileName, strNewWaveFileName);

                        // rename data file
                        txtFilename = strFileName.Substring(0, strFileName.Length - 4) + ".TXT";
                        strNewDataFileName = strFileName.Substring(0, strFileName.LastIndexOf('/') + 1) + editTextView.Text + ".TXT";
                        comFun.RenameFile(txtFilename, strNewDataFileName);
                    }
                    
                    FilesChanged(this, new EventArgs());
                    DisplayDefaultMenu();

                }
                catch (Exception e1)
                {
                    AndroidX.AppCompat.App.AlertDialog.Builder dlErrorMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

                    dlErrorMessage.SetTitle("Error");
                    dlErrorMessage.SetMessage("Could not rename the file. EXception :" + e1.Message);
                    dlErrorMessage.Show();
                }

            });

            alert.SetNegativeButton("Cancel", (senderAlert, argus) =>
            {

            });

            //et.
            global::Android.App.Dialog dialog = alert.Create();
            dialog.Show();
        }

        internal void SaveListViewFromFragment(FileListFragment fileList)
        {
            m_frag = fileList;
        }

        internal void FileSelected(ListView lstView, string strFileName)
        {

            m_lstFilesSelected.Add(strFileName);

            if (m_lstFilesSelected.Count == 1)
            {
                DisplaySingleFileSelectedMenu();
            }
            else
            {
                DisplayMultipleFileSelectedMenu();
            }
        }

        internal void DisplayMultipleFileSelectedMenu()
        {

            SupportActionBar.Title = m_lstFilesSelected.Count + " Selec..";

            for (int i = 0; i < m_intNumMultipleFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(true);
            }


            for (int i = m_intNumMultipleFileMenuItems; i < m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(false);
            }


            for (int i = m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i < menu.Size(); i++)
            {
                menu.GetItem(i).SetVisible(false);
            }
        }


        internal void DisplaySingleFileSelectedMenu()
        {


            SupportActionBar.Title = m_lstFilesSelected.Count + " Selec..";

            for (int i = 0; i < m_intNumMultipleFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(false);
            }


            for (int i = m_intNumMultipleFileMenuItems; i < m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(true);
            }


            for (int i = m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i < menu.Size(); i++)
            {
                menu.GetItem(i).SetVisible(false);
            }


        }


        internal void FileItemDeSelected(ListView lstView, string strFileName)
        {
            if (m_lstFilesSelected.Count == 0)
            {
                return;
            }
            else
            {
                m_lstFilesSelected.Remove(strFileName);
            }

            if (m_lstFilesSelected.Count == 0)
            {
                DisplayDefaultMenu();
                return;
            }
            else if (m_lstFilesSelected.Count == 1)
            {

                DisplaySingleFileSelectedMenu();
            }


        }



        internal void DisplayDefaultMenu()
        {
            //if (m_blFilePickerMenuDisplayed)
            //{
            View listItemView;

            for (int i = 0; i < m_frag.ListView.Count; i++)
            {
                listItemView = m_frag.ListView.GetChildAt(i);

                if (listItemView != null)
                {
                    (listItemView as LinearLayout).SetBackgroundColor(Color.Black);
                }

                // (listItemView as LinearLayout).Background.Alpha = 100;

            }

            m_lstFilesSelected.Clear();

            SupportActionBar.Title = "TTtuner";

            for (int i = 0; i < m_intNumMultipleFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(false);
            }


            for (int i = m_intNumMultipleFileMenuItems; i < m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i++)
            {
                menu.GetItem(i).SetVisible(false);
            }


            for (int i = m_intNumMultipleFileMenuItems + m_intNumFileMenuItems; i < menu.Size(); i++)
            {
                menu.GetItem(i).SetVisible(true);
            }
            //  }

        }


        private void ShareFiles()
        {
            //ArrayList<Uri> imageUris = new ArrayList<Uri>();
#if Trial_Version
            AndroidX.AppCompat.App.AlertDialog.Builder dlMessage = new AndroidX.AppCompat.App.AlertDialog.Builder(this);

            dlMessage.SetTitle("Share Feature not enabled");
            dlMessage.SetMessage("This feature is not enabled in the trial version of TTtuner. The full version can be purchased in the Google Play Store.");
            dlMessage.SetPositiveButton("OK", (senderAlert, argus) => { });


            dlMessage.Show();


#else
            CommonFunctions cmFunc = new CommonFunctions();

            var intent = new Intent(Intent.ActionSendMultiple);
            intent.SetType(MediaStoreHelper.MIMETYPE_WAV);

            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            string auth;

//#if Trial_Version
//           auth = "com.music.Tunetracker";
//#else
            auth = "com.full.Tunetracker";
            //#endif


            
            var uris = new List<IParcelable>();
            m_lstFilesSelected.ForEach(file =>
            {
                string extenstion = cmFunc.GetFileNameExtension(file);

                if (extenstion == "STT")
                {
                    ExportStatsFile(file, uris);
                }
                else
                {
                   ExportWaveFile(file, uris);                   
                }

            });

            intent.PutParcelableArrayListExtra(Intent.ExtraStream, uris);

            var intentChooser = Intent.CreateChooser(intent, "Share via");

            StartActivityForResult(intentChooser, 1);
             DisplayDefaultMenu();
#endif

        }

        private void ExportWaveFile(string file, List<IParcelable> uris)
        {
            CommonFunctions cmFunc = new CommonFunctions();
            // FLAG_GRANT_READ_URI_PERMISSION
            // set up wave file in temp dir
            string strFileName = file.Substring(file.LastIndexOf('/') + 1, file.LastIndexOf('.') - file.LastIndexOf('/') - 1);
            string extenstion = cmFunc.GetFileNameExtension(file);

            Java.IO.File temporaryFile = Java.IO.File.CreateTempFile(strFileName + "-", ".wav", ExternalCacheDir);
            Java.IO.File fl1 = new Java.IO.File(file);

            // copy file to new location
            FileChannel src = new FileInputStream(fl1).Channel;
            FileChannel dest = new FileOutputStream(temporaryFile).Channel;

            dest.TransferFrom(src, 0, src.Size());
            fl1.Dispose();

            Java.IO.File temporaryFile2 = Java.IO.File.CreateTempFile(strFileName + "-", ".txt", ExternalCacheDir);

            try
            {
                string fileOutput = GetStatsText(strFileName + ".txt");
                cmFunc.CopyStringToFile(fileOutput, temporaryFile2);

                var statsUri = AndroidX.Core.Content.FileProvider.GetUriForFile(ApplicationContext,
                    PackageName + ".provider", temporaryFile2);

                uris.Add(statsUri);
                var waveUri = AndroidX.Core.Content.FileProvider.GetUriForFile(ApplicationContext,
                PackageName + ".provider", temporaryFile);

                uris.Add(waveUri);
            }
            catch (IOException e)
            {
                throw e;
            }
            finally
            {
                temporaryFile.DeleteOnExit();
                temporaryFile2.DeleteOnExit();
            }
        }

        private void ExportStatsFile(string file, List<IParcelable>  uris)
        {
            CommonFunctions cmFunc = new CommonFunctions();
            string strFileName = file.Substring(file.LastIndexOf('/') + 1, file.LastIndexOf('.') - file.LastIndexOf('/') - 1);
            
            // set up Stats file in temp dir
            //strFileName = strFileName +"_data";
            Java.IO.File temporaryFile2 = Java.IO.File.CreateTempFile(strFileName + "-", ".txt", ExternalCacheDir);

            try
            {
                string fileOutput = GetStatsText(strFileName + ".stt");
                cmFunc.CopyStringToFile(fileOutput, temporaryFile2);

                var statsUri = AndroidX.Core.Content.FileProvider.GetUriForFile(ApplicationContext,
                    PackageName + ".provider", temporaryFile2);

                uris.Add(statsUri);
            }
            catch (IOException e)
            {
                throw e;
            }
            finally
            {
                temporaryFile2.DeleteOnExit();
            }
        }

        private string GetStatsText(string filename)
        {       

            DataPointHelper<Serializable_DataPoint> dataHelperFrq = DataPointCollection.Frq;
            DataPointHelper<Serializable_DataPoint_Std> dataHelperDcb = DataPointCollection.Dcb;
            NoteStatsGenerator ntStatGen = new NoteStatsGenerator(Settings.MinNumberOfSamplesForNote);
            CommonFunctions comFun = new CommonFunctions();
            string strReturn = "";
            bool outputDcb = false;

            dataHelperFrq.LoadDataPointsFromFile(filename, true);
            if (comFun.DoesDcbFileExistForThisFreqFile(System.IO.Path.Combine(Common.Settings.DataFolder, filename)))
            {
                outputDcb = true;
                dataHelperDcb.LoadDataPointsFromFile(comFun.GetDcbFileNameForThisFreqFile(filename));
            }

            strReturn = ntStatGen.GetStatsAsString(dataHelperFrq, dataHelperDcb);
            return strReturn;

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_llFiles.RemoveAllViews();
            cEventHelper.RemoveAllEventHandlers(this.FilesChanged);

        }

        private void FinishActvity()
        {

            Finish();
        }

        public override void OnBackPressed()
        {

            // always go back to main activity

            StartActivity(typeof(MainActivity));

        }
    }
}
