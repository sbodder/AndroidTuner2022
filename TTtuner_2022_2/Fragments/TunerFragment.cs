using System;
using Android.App;
using Android.OS;
using TTtuner_2022_2.PopUpDialogFragments;

using global::Android.Views;
using global::Android.Widget;
using Android.Graphics;
using TTtuner_2022_2.Common;

namespace TTtuner_2022_2.Fragments
{
    class TunerFragment : AndroidX.Fragment.App.Fragment
    {
        private TextView m_txtNote;
        private TunerScalesView m_tunerScalesView = null;
        private LinearLayout m_llContainingTunerScalesView;
        private TextView m_txtFreq;
        private TextView m_txtCentsDev;
        private TextView m_txtTuningSpecsLeft;
        private TextView m_txtTuningSpecsRight;
        private TextView m_txtTimeElapsed;
        private ProgressBar m_prgBAr;

        public bool SetupComplete { get; set; } = false;

        public static TunerFragment NewInstance()
        {
            var tunerFrag = new TunerFragment { Arguments = new Bundle() };
            return tunerFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            View view = inflater.Inflate(Resource.Layout.Tunerfragment, container, false);

            m_txtNote = view.FindViewById<TextView>(Resource.Id.txtNote1);
            m_tunerScalesView = view.FindViewById<TunerScalesView>(Resource.Id.tunerScalesView);
            m_llContainingTunerScalesView = view.FindViewById<LinearLayout>(Resource.Id.llContainingTunerScalesView);
            m_txtFreq = view.FindViewById<TextView>(Resource.Id.txtFreq);
            m_txtCentsDev = view.FindViewById<TextView>(Resource.Id.txtCentsDev);
            m_txtTuningSpecsLeft = view.FindViewById<TextView>(Resource.Id.txtTuningSpecsLeft);
            m_txtTuningSpecsRight = view.FindViewById<TextView>(Resource.Id.txtTuningSpecsRight);
            m_txtTimeElapsed = view.FindViewById<TextView>(Resource.Id.txtTimeElapsed);
            m_txtTuningSpecsRight.LongClick += OnA4longClick;

            m_prgBAr = view.FindViewById<ProgressBar>(Resource.Id.PrgBar);
            m_prgBAr.Visibility = ViewStates.Gone;

            SetupComplete = true;
            return view;
        }

        internal void UpdateTunerText(Common.NoteEventArgs e)
        {
            m_txtNote.Text = e.Note;
            m_tunerScalesView.CentsDeviation = e.CentsCloseness;
            m_txtFreq.Text = string.Format("{0:0.00}Hz", e.Pitch);
            m_txtCentsDev.Text = string.Format("{0:0} Cts", e.CentsCloseness);
        }

        internal void UpdateTimeElapsedText(string time)
        {
            m_txtTimeElapsed.Text = time;
        }

        internal void DisplayProgressBar(string time)
        {
            m_txtTimeElapsed.Visibility = ViewStates.Gone;
            m_prgBAr.Visibility = ViewStates.Visible;
        }

        internal void UpdateTunerScales()
        {
            if ( (m_tunerScalesView!= null) && (m_tunerScalesView.IsShown) && (m_tunerScalesView.Visibility == ViewStates.Visible))
            {
                // Logger.Info(Common.CommonFunctions.APP_NAME, "Running invalidate on view ...");
                if (Math.Abs((int)m_tunerScalesView.GetAvgValueOfBuffer()) > 25)
                {
                    m_txtNote.SetTextColor(Color.Rgb(0xFF, 0xFF, 0x99));
                }
                else
                {
                    m_txtNote.SetTextColor(Color.Rgb(0x99, 0xcc, 0));
                }
                m_tunerScalesView.Invalidate();
            }
        }

        internal void OnA4longClick(object sender, EventArgs e)
        {
            var dialog = PopUpWithSeekerDialog.NewInstance(390, 500, 0.1, Common.Settings.A4refDouble);
            dialog.Title = "A4 Reference Adjustment (Hz)";           

            dialog.Description = "";
            dialog.ValueSet += (sender2, value) =>
            {
                Common.Settings.UpdateA4refButDontSavetoDisk(value);
                UpdateSettingsDisplay();
                //this.Recreate();
            };
            dialog.Show(this.FragmentManager, "dialog");
        }

        public override void OnResume()
        {
            base.OnResume();
            UpdateSettingsDisplay();
        }

        internal void UpdateSettingsDisplay()
        {
            CommonFunctions cmFunc = new CommonFunctions();

            m_txtTuningSpecsLeft.Text = cmFunc.TruncateStringRight(Common.Settings.TuningSystemAndRootScale, 20) + "\n";
            m_txtTuningSpecsLeft.Text += Common.Settings.Transpose;
            m_txtTuningSpecsRight.Text = Common.Settings.SampleRate + " Hz\n" + Common.Settings.A4ref + "Hz";
        }

        internal void Destroy()
        {
            m_llContainingTunerScalesView.RemoveAllViews();
            m_tunerScalesView.Destroy();
            m_txtTuningSpecsRight.LongClick -= OnA4longClick;
        }
    }
}