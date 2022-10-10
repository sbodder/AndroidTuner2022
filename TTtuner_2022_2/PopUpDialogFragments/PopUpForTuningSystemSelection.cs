using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.AppCompat.App;
using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;
using System.Reflection;

namespace TTtuner_2022_2.PopUpDialogFragments
{

    internal class PopUpForTuningSystemSelectionDialog : DialogFragment
    {

        Spinner spinnerScale, spinnerTuningSystem;

        TextView textScaleSelection;

        internal event EventHandler<List<string>> ValueSet;

        private string m_strTuningSystemSelection;

        private string m_strScaleSelected;

        internal static PopUpForTuningSystemSelectionDialog NewInstance(string strTuningSys, string strScale)
        {
            var dialogFragment = new PopUpForTuningSystemSelectionDialog(strTuningSys, strScale);
            return dialogFragment;
        }


        internal PopUpForTuningSystemSelectionDialog()
        {

        }


        internal PopUpForTuningSystemSelectionDialog(string strTuningSys, string strScale)
        {
            m_strTuningSystemSelection = strTuningSys;

            m_strScaleSelected = strScale;
        }

       
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Begin building a new dialog.
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);

      

            //Get the layout inflater
            var inflater = Activity.LayoutInflater;

            List<string> lstTuningSystemName = Common.Settings.TuningSystemsList;


            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.PopUpForTuningSystemSelection, null);


            if (dialogView != null)
            {

                spinnerTuningSystem = dialogView.FindViewById<Spinner>(Resource.Id.SpinerTunningSystem);

                ArrayAdapter adapter1 = new ArrayAdapter(Activity, global::Android.Resource.Layout.SimpleSpinnerDropDownItem, lstTuningSystemName);
                //var adapter1 = ArrayAdapter.CreateFromResource(
                //        Activity, Resource.Array.tuning_array, Android.Resource.Layout.SimpleSpinnerItem);

                adapter1.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinnerTuningSystem.Adapter = adapter1;

                for (int i = 0; i < spinnerTuningSystem.Adapter.Count; i++)
                {
                    var item = spinnerTuningSystem.Adapter.GetItem(i);
                    if (m_strTuningSystemSelection == item.ToString())
                    {
                        spinnerTuningSystem.SetSelection(i);
                        break;
                    }

                }

                spinnerTuningSystem.ItemSelected += (sender, args) =>
                {
                    //if ( spinnerTuningSystem.Adapter.GetItem( args.Position).ToString() == "Just Intonation")
                    //{

                    //}
                    HideScaleSelectionControlsBasedonTuningSystemSelection();
                };




                spinnerScale = dialogView.FindViewById<Spinner>(Resource.Id.spinner4);

                var adapter = ArrayAdapter.CreateFromResource(
                         Activity, Resource.Array.ScaleSelection_array, global::Android.Resource.Layout.SimpleSpinnerItem);

                adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

                spinnerScale.Adapter = adapter;


            

                for (int i = 0; i < spinnerScale.Adapter.Count; i++)
                {
                    var item = spinnerScale.Adapter.GetItem(i);
                    if (m_strScaleSelected == item.ToString())
                    {
                        spinnerScale.SetSelection(i);
                        break;
                    }

                }


                textScaleSelection = dialogView.FindViewById<TextView>(Resource.Id.textScaleSelection);


                HideScaleSelectionControlsBasedonTuningSystemSelection();



                builder.SetView(dialogView);
                builder.SetPositiveButton("OK", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancel", HandleNegativeButtonClick);
            }


            //Create the builder 
            var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        private void HideScaleSelectionControlsBasedonTuningSystemSelection()
        {
            if (spinnerTuningSystem.SelectedItem.ToString() == "Equal Temperament")
            {
                // Hide the scale selection controls
                textScaleSelection.Visibility = ViewStates.Gone;
                spinnerScale.Visibility = ViewStates.Gone;
            }
            else
            {
                // Hide the scale selection controls
                textScaleSelection.Visibility = ViewStates.Visible;
                spinnerScale.Visibility = ViewStates.Visible;
            }
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AndroidX.AppCompat.App.AlertDialog)sender;
            List<string> lst = new List<string>();

           
            lst.Add(spinnerTuningSystem.SelectedItem.ToString());
            lst.Add(spinnerScale.SelectedItem.ToString());


            Common.CommonFunctions comFun = new Common.CommonFunctions();


            ValueSet(this, lst);


            dialog.Dismiss();

        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {

        }

    }



}