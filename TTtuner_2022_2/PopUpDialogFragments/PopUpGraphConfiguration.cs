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
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.PopUpDialogFragments
{
    internal class PopUpGraphConfiguration : DialogFragment
    {
        Activity m_act;
        bool m_blFreqOn;
        bool m_blDecibelOn;
        string m_strPrompt;
        internal event EventHandler<string[]> OkPressed;
        string m_strGraphOverviewValue;
        Switch m_swhNote;
        Switch m_swhDecible;
        TextView m_textPrompt;
        ArrayAdapter _adapter;

        Spinner m_spGraphOverview;

        internal static PopUpGraphConfiguration NewInstance(string strPrompt, string strGraphOverviewValue, bool blFreqOn, bool blDecibelOn, Activity act)
        {
            var dialogFragment = new PopUpGraphConfiguration(strPrompt, strGraphOverviewValue, blFreqOn, blDecibelOn, act);
            return dialogFragment;
        }

        internal PopUpGraphConfiguration(string strPrompt, string strGraphOverviewValue, bool blFreqOn, bool blDecibelOn, Activity act)
        {
            m_blFreqOn = blFreqOn;
            m_blDecibelOn = blDecibelOn;
            m_strPrompt = strPrompt;
            m_act = act;
            m_strGraphOverviewValue = strGraphOverviewValue;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            int graphOverviewOffset;
            CommonFunctions comFunc = new CommonFunctions();

            int intNumDigits;

            // Begin building a new dialog.
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
            //Get the layout inflater
            var inflater = Activity.LayoutInflater;


            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.PopUpForGraphConfiguration, null);

            m_swhNote = dialogView.FindViewById<Switch>(Resource.Id.SwitchNotesGraphOption);
            m_swhNote.Checked = m_blFreqOn;
            m_swhNote.Click += SwitchValueChanged;

            m_swhDecible = dialogView.FindViewById<Switch>(Resource.Id.SwitchDecibelGraphOption);
            m_swhDecible.Checked = m_blDecibelOn;
            m_swhDecible.Click += SwitchValueChanged;

            m_textPrompt = dialogView.FindViewById<TextView>(Resource.Id.textGraphConfigurationPrompt);
            m_textPrompt.Text = m_strPrompt;

            if (dialogView != null)
            {
                builder.SetView(dialogView);
                builder.SetCancelable(false);
                builder.SetPositiveButton("OK", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancel", HandleNegativeButtonClick);
            }

            // Setup the spinner

            m_spGraphOverview = dialogView.FindViewById<Spinner>(Resource.Id.spinnerGraphOverview);
            _adapter = ArrayAdapter.CreateFromResource(m_act, Resource.Array.GraphOverview_array, Resource.Layout.spinner_item_white);

            _adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

            m_spGraphOverview.Adapter = _adapter;

            // set the value 
            graphOverviewOffset = comFunc.GetIndexOfItemInstringArray(
                m_act.Resources.GetStringArray(Resource.Array.GraphOverview_array),
                m_strGraphOverviewValue);

            m_spGraphOverview.SetSelection(graphOverviewOffset);

            m_spGraphOverview.ItemSelected += GraphOverviewSpinnerItemSelected;

            //Create the builder 
            var dialog = builder.Create();

            dialog.SetCanceledOnTouchOutside(true);
            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        void GraphOverviewSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs args)
        {
#if Trial_Version
            m_spGraphOverview.SetSelection(0);
            Toast.MakeText(m_act, "Only available in the full version of TTtuner available in the Google Play Store", ToastLength.Long).Show();
            return;
#else
            string itemSel = m_act.Resources.GetStringArray(Resource.Array.GraphOverview_array)[args.Position];
            CommonFunctions comFunc = new CommonFunctions();

            m_strGraphOverviewValue = itemSel;

            if ((itemSel == "Notes Graph") && (!m_swhNote.Checked))
            {
                m_swhNote.Checked = true;
            }
            else if ((itemSel == "Decibel Graph") && (!m_swhDecible.Checked))
            {
                m_swhDecible.Checked = true;
            }
#endif
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AndroidX.AppCompat.App.AlertDialog)sender;

            string[] results = new string[3];
            results[0] = m_swhNote.Checked.ToString();
            results[1] = m_swhDecible.Checked.ToString();
            results[2] = m_strGraphOverviewValue;
            OkPressed?.Invoke(this, results);
            dialog.Dismiss();
        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
        }


        public void SwitchValueChanged(object sender, EventArgs evt)
        {
            int graphOverviewOffset;
            CommonFunctions comFunc = new CommonFunctions();

#if Trial_Version
            Toast.MakeText(m_act, "Only available in the full version of TTtuner available in the Google Play Store", ToastLength.Long).Show();
            m_swhNote.Checked = true;
            m_swhDecible.Checked = false;
            return;

#else
            // check both have not been switch off
            if (!m_swhNote.Checked && !m_swhDecible.Checked)
            {
                (sender as Switch).Checked = !(sender as Switch).Checked;
            }

            // check that the graph selected for overview has been switch on
            if ((m_strGraphOverviewValue == "Notes Graph") && !m_swhNote.Checked)
            {
                m_strGraphOverviewValue = "Decibel Graph";
            }
            else if ((m_strGraphOverviewValue == "Decibel Graph") && !m_swhDecible.Checked)
            {
                m_strGraphOverviewValue = "Notes Graph";
            }

            // set the value 
            graphOverviewOffset = comFunc.GetIndexOfItemInstringArray(
                m_act.Resources.GetStringArray(Resource.Array.GraphOverview_array),
                m_strGraphOverviewValue);

            m_spGraphOverview.SetSelection(graphOverviewOffset);
#endif

        }
    }

}