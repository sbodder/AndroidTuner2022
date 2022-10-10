using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using System.Reflection;
using static TTtuner_2022_2.Audio.AudioFileImporter;
using TTtuner_2022_2.Audio;
using System.Threading.Tasks;
using AndroidX.Fragment.App;

namespace TTtuner_2022_2.PopUpDialogFragments
{
    internal class PopUpFileProgressDialog : DialogFragment
    {
        TextView textDescriptionSeeker1;
        TextView textDescriptionSeeker2;
        TextView textTitle;
        TextView textSeekVal;
        SeekBar seekbarFilePercent;
        SeekBar seekbarNumFiles;

        AudioFileImporter m_wvImporter;

        Button buttonPlus, buttonMinus;

        Action m_CallWhenComplete;

        global::Android.App.Activity m_act;


        internal string DescriptionSeek1 { get; set; }

        internal string DescriptionSeek2 { get; set; }

        internal string Title { get; set; }


        internal event EventHandler<string> ValueSet;

        internal int m_numFiles { get; set; }

        //internal double m_seekBarMaxValue { get; set; }

        //internal double m_seekBarIncrementValue { get; set; }

        //internal double m_seekBarCurrentValue { get; set; }

        internal static PopUpFileProgressDialog NewInstance(string title, string descriptionSeek1, string descriptionSeek2, int numFiles, AudioFileImporter wvImport, Action CallWhenComplete, global::Android.App.Activity act)
        {
            var dialogFragment = new PopUpFileProgressDialog(title, descriptionSeek1, descriptionSeek2, numFiles, wvImport, CallWhenComplete, act);
            return dialogFragment;
        }

        public async override void OnResume()
        {
            base.OnResume();

            await Task.Delay(10);
        }

        override public void OnSaveInstanceState(Bundle outState)
        {
            //No call for super(). Bug on API Level > 11.
        }

        internal PopUpFileProgressDialog(string title, string descriptionSeek1, string descriptionSeek2, int numFiles, AudioFileImporter wvImport, Action CallWhenComplete, global::Android.App.Activity act)
        {
            m_numFiles = numFiles;

            m_wvImporter = wvImport;

            m_wvImporter.FileProgress += UpdateWithFileProgress;
            m_CallWhenComplete = CallWhenComplete;
            Title = title;
            DescriptionSeek1 = descriptionSeek1;
            DescriptionSeek2 = descriptionSeek2;

            m_act = act;
        }


        public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            int intNumDigits;

            // Begin building a new dialog.
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
            var builder2 = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
            //Get the layout inflater
            var inflater = Activity.LayoutInflater;


            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.PopUpFileProgress, null);

            textDescriptionSeeker1 = dialogView.FindViewById<TextView>(Resource.Id.textDescriptionSeek1);
            textDescriptionSeeker2 = dialogView.FindViewById<TextView>(Resource.Id.textDescriptionSeek2);

            textTitle = dialogView.FindViewById<TextView>(Resource.Id.textTitle);
            seekbarFilePercent = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarFilePercent);

            seekbarNumFiles = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarFileNumber);


            textTitle.Text = this.Title;

            textDescriptionSeeker1.Text = this.DescriptionSeek1;

            textDescriptionSeeker2.Text = this.DescriptionSeek2 + " 1 of " + m_numFiles;

            textSeekVal = dialogView.FindViewById<TextView>(Resource.Id.textSeekValue);

            seekbarFilePercent.Max = 100;

            seekbarNumFiles.Max = m_numFiles;

            seekbarFilePercent.Progress = 0;

            seekbarNumFiles.Progress = 0;

            //GetWindow().setFlags(WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE,
            //        WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE);

            if (dialogView != null)
            {
                builder.SetView(dialogView);
                builder.SetCancelable(false);
            }




            //Create the builder 
            var dialog = builder.Create();

            dialog.SetCanceledOnTouchOutside(false);

            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        internal void UpdateWithFileProgress(object sender, FileProgressArgs e)
        {
            m_act.RunOnUiThread(() =>
            {
                if (e.Complete)
                {
                    m_wvImporter.FileProgress -= UpdateWithFileProgress;
                    m_CallWhenComplete();
                    this.DismissAllowingStateLoss();
                }
                else
                {
                    seekbarFilePercent.Progress = e.PercentOFFileComplete;

                    seekbarNumFiles.Progress = e.NumberOfFilesCompleted + 1;

                    textDescriptionSeeker2.Text = this.DescriptionSeek2 + " " + (e.NumberOfFilesCompleted + 1) + " of " + m_numFiles;
                }


            });

        }
    }
}