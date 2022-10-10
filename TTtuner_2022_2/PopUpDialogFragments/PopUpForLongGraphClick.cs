using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using global::Android.Views;
using global::Android.Widget;

namespace TTtuner_2022_2.PopUpDialogFragments
{
    internal class PopUpForLongGraphClick : DialogFragment
    {
        // max ms value is 5000 ms and the max postion on the seek bar is 100 
        // => MULT_FACTOR is 50
        const int HORIZONTAL_MULT_FACTOR = 50;
        Activity m_act;
        bool m_blSnapToNoteSwtichOn;
        bool m_blDefaultZoomSwtichOn;
        int _intZoomXms;
        int _intZoomY;
        internal event EventHandler<bool> SnapToNoteSwitchChanged;
        internal event EventHandler<bool> DefaultZoomSwitchChanged;
        internal event EventHandler<int> ZoomXChanged;
        internal event EventHandler<float> ZoomYChanged;

        Switch m_SnapToNoteSwhControl;
        Switch m_DefaultZoomSwhControl;
        SeekBar _ZoomHorizCntrl;
        SeekBar _ZoomVerticalCntrl;

        internal static PopUpForLongGraphClick NewInstance(bool blSnapToNoteSwitchOn,
                                                            bool blDefaultZoomSwitchOn,
                                                            int intZoomXms, float flZoomY, Activity act)
        {
            var dialogFragment = new PopUpForLongGraphClick(blSnapToNoteSwitchOn, blDefaultZoomSwitchOn, intZoomXms, flZoomY, act);
            return dialogFragment;
        }

        internal PopUpForLongGraphClick(bool blSnapToNoteSwitchOn,
                                        bool blDefaultZoomSwitchOn,
                                        int intZoomXms, float flZoomY, Activity act)
        {
            m_blSnapToNoteSwtichOn = blSnapToNoteSwitchOn;
            m_blDefaultZoomSwtichOn = blDefaultZoomSwitchOn;
            _intZoomXms = (int) (intZoomXms);
            _intZoomY = (int) (flZoomY * 100);
            m_act = act;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        
            // Begin building a new dialog.
            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, Resource.Style.MyTransparentDialog);
            //Get the layout inflater
            var inflater = Activity.LayoutInflater;

            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.PopUpGraphLongClickOptions, null);

            m_SnapToNoteSwhControl = dialogView.FindViewById<Switch>(Resource.Id.SwitchGraphOption);
            m_SnapToNoteSwhControl.Checked = m_blSnapToNoteSwtichOn;
            m_SnapToNoteSwhControl.Click += SnapToNoteSwitchValueChanged;

            m_DefaultZoomSwhControl = dialogView.FindViewById<Switch>(Resource.Id.SwitchDefaultZoomOption);
            m_DefaultZoomSwhControl.Checked = m_blDefaultZoomSwtichOn;
            m_DefaultZoomSwhControl.Click += DefaultZoomSwitchValueChanged;

            _ZoomVerticalCntrl = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarZoomYpercnt);
            _ZoomHorizCntrl = dialogView.FindViewById<SeekBar>(Resource.Id.seekBarZoomXpercnt);

            _ZoomVerticalCntrl.Max = 100;
            _ZoomHorizCntrl.Max = 100;

            _ZoomVerticalCntrl.Progress = _intZoomY;
            _ZoomHorizCntrl.Progress =  Math.Min(99, _intZoomXms / HORIZONTAL_MULT_FACTOR) ;

            _ZoomVerticalCntrl.ProgressChanged += ZoomVerticalCntrl_ProgChanged;
            _ZoomHorizCntrl.ProgressChanged += ZoomHorizCntrl_ProgChanged;

            _ZoomVerticalCntrl.Enabled = !m_blDefaultZoomSwtichOn;
            _ZoomHorizCntrl.Enabled = !m_blDefaultZoomSwtichOn;
            
            if (dialogView != null)
            {
                builder.SetView(dialogView);
                builder.SetCancelable(false);
            }
            
            //Create the builder 
            var dialog = builder.Create();
            
            dialog.SetCanceledOnTouchOutside(true);
            
            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        private void ZoomHorizCntrl_ProgChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            ZoomXChanged?.Invoke(this, _ZoomHorizCntrl.Progress * HORIZONTAL_MULT_FACTOR);
        }

        private void ZoomVerticalCntrl_ProgChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            ZoomYChanged?.Invoke(this, _ZoomVerticalCntrl.Progress / 100f);
        }

        private void DefaultZoomSwitchValueChanged(object sender, EventArgs e)
        {
            _ZoomVerticalCntrl.Enabled = !m_DefaultZoomSwhControl.Checked;
            _ZoomHorizCntrl.Enabled = !m_DefaultZoomSwhControl.Checked;
            DefaultZoomSwitchChanged?.Invoke(this, m_DefaultZoomSwhControl.Checked);
        }

        public void SnapToNoteSwitchValueChanged(object sender, EventArgs evt)
        {
            SnapToNoteSwitchChanged?.Invoke(this, m_SnapToNoteSwhControl.Checked);
        }
    }
}