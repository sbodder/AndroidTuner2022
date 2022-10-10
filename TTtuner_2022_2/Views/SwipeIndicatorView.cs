using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using global::Android.Views;
using global::Android.Widget;
using static Android.Graphics.Paint;


namespace TTtuner_2022_2
{
    class SwipeIndicatorView : View
    {
        Context m_Context;

        public float PercentHeight { get; set; } = 0.33f;
        public bool RoundSideFacingLeft { get; set; } = true;
        public bool ShowOval { get; set; } = true;

        public SwipeIndicatorView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public SwipeIndicatorView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        private Paint m_pntColor = new Paint
        {
            AntiAlias = true,
            StrokeWidth = 8.0f,
            Color = Color.Rgb(128, 128, 128),

            TextSize = 30
        };

        private void Initialize(Context context)
        {
            m_Context = context;
            m_pntColor.SetStyle(Style.Fill);
        }

        protected override void OnDraw(Canvas canvas)
        {
            try
            {
                base.OnDraw(canvas);
                DrawOval(canvas);
            }
            catch (Exception e1)
            {
                Toast.MakeText(m_Context, "Exception drawing", ToastLength.Long).Show();
            }
        }

        private void DrawOval(Canvas canvas)
        {
            float top = (Height - (Height * PercentHeight)) / 2f ;
            float bottom = top + (Height * PercentHeight);

            if (!ShowOval)
            {
                return;
            }
            if (RoundSideFacingLeft)
            {
                canvas.DrawOval(0f, top, Width * 2, bottom, m_pntColor);
            }
            else
            {
                canvas.DrawOval(-Width, top, Width, bottom, m_pntColor);
            }
        }
    }
}
