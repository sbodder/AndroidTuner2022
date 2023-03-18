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
using TTtuner_2022_2.Fragments;
using Java.Lang;

namespace TTtuner_2022_2.Adapters
{
    class MainFragmentAdapter : AndroidX.Fragment.App.FragmentPagerAdapter
    {
        private TunerFragment m_tunFg;
        private StatsViewFragment m_staFg;

        public MainFragmentAdapter(AndroidX.Fragment.App.FragmentManager fm, AppCompatActivity act)
            : base(fm)
        {
            m_tunFg = TunerFragment.NewInstance();
            m_staFg = StatsViewFragment.NewInstance( true, false, true);

        }

        public override int Count
        {
            get { return 2; }
        }

        public override AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                default:
                case 0: return m_tunFg;
                case 1: return m_staFg;                
            }
           
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            switch (position)
            {
                default:
                case 0: return new Java.Lang.String("Tuner");
                case 1: return new Java.Lang.String("Stats");
            }
        }

        public override void DestroyItem(View container, int position, Java.Lang.Object view)
        {
            base.DestroyItem(container, position, view);

            var viewPager = container.JavaCast<AndroidX.ViewPager.Widget.ViewPager>();
            viewPager.RemoveView(view as View);

            switch (position)
            {
                default:
                case 0: m_tunFg = null; break;
                case 1: m_staFg.Destroy();  m_staFg = null; break;
            }
        }
    }
    
}