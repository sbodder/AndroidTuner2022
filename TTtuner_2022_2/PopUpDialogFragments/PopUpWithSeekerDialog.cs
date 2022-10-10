using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using System.Reflection;

namespace TTtuner_2022_2.PopUpDialogFragments
{
    internal class PopUpWithSeekerDialog : AndroidX.Fragment.App.DialogFragment
    {
        TextView textDescription;
        TextView textTitle;
        TextView textSeekVal;
        SeekBar seekbar;

        Button buttonPlus, buttonMinus;
        internal string Description { get; set; }

        internal string Title { get; set; }
        internal event EventHandler<string> ValueSet;
        internal double m_seekBarMinValue { get; set; }

        internal double m_seekBarMaxValue { get; set; }

        internal double m_seekBarIncrementValue { get; set; }

        internal double m_seekBarCurrentValue { get; set; }


        internal static PopUpWithSeekerDialog NewInstance(double seekBarMinValue, double seekBarMaxValue, double SeekBarIncrementValue, double seekBarCurrentValue)
        {
            var dialogFragment = new PopUpWithSeekerDialog(seekBarMinValue, seekBarMaxValue, SeekBarIncrementValue, seekBarCurrentValue);
            return dialogFragment;
        }

        internal PopUpWithSeekerDialog(double seekBarMinValue, double seekBarMaxValue, double SeekBarIncrementValue, double seekBarCurrentValue)
        {
            m_seekBarMinValue = seekBarMinValue;
            m_seekBarMaxValue = seekBarMaxValue;
            m_seekBarIncrementValue = SeekBarIncrementValue;
            m_seekBarCurrentValue = seekBarCurrentValue;
        }


        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            int intNumDigits;

            // Begin building a new dialog.
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);

            //Get the layout inflater
            var inflater = Activity.LayoutInflater;


            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.PopUpWithSeekerBar, null);


            textDescription = dialogView.FindViewById<TextView>(Resource.Id.textDescription);
            textTitle = dialogView.FindViewById<TextView>(Resource.Id.textTitle);
            seekbar = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarPopUp);


            textTitle.Text = this.Title;

            textDescription.Text = this.Description;

            textSeekVal = dialogView.FindViewById<TextView>(Resource.Id.textSeekValue);

            seekbar.Max = Convert.ToInt32((m_seekBarMaxValue - m_seekBarMinValue) / m_seekBarIncrementValue);

            textSeekVal.Text = string.Format("{0:0.#}", m_seekBarCurrentValue);

            seekbar.Progress = Convert.ToInt32((m_seekBarCurrentValue - m_seekBarMinValue) / m_seekBarIncrementValue);


            seekbar.ProgressChanged += (sender, args) =>
            {
                textSeekVal.Text = (m_seekBarMinValue + (seekbar.Progress * m_seekBarIncrementValue)).ToString();
            };

            buttonMinus = dialogView.FindViewById<Button>(Resource.Id.ButtonMinus);

            buttonPlus = dialogView.FindViewById<Button>(Resource.Id.ButtonPlus);


            buttonMinus.Click += (sender, args) =>
           {
               seekbar.Progress--;

           };

            buttonPlus.Click += (sender, args) =>
            {
                seekbar.Progress++;

            };

            if (dialogView != null)
            {


                builder.SetView(dialogView);
                builder.SetPositiveButton("OK", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancel", HandleNegativeButtonClick);
            }


            //Create the builder 
            var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AndroidX.AppCompat.App.AlertDialog)sender;



            Common.CommonFunctions comFun = new Common.CommonFunctions();


            ValueSet(this, textSeekVal.Text);

            dialog.Dismiss();

        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {

        }
    }
}