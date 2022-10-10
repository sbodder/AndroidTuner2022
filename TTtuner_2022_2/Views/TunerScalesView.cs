using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using global::Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using global::Android.Views;
using global::Android.Widget;
using Android.Graphics;
using static Android.Graphics.Paint;
using System.Reflection;

namespace TTtuner_2022_2
{
    public class TunerScalesView : View
    {
        //internal double PercentCloseness { get; set; }
        internal double CentsDeviation { get; set; }
        Context m_Context;
        private int m_intMaxCents = 50;

        const int m_BUFFER_ARRAY_SIZE = 10;
        private int intCurrentArrayIndex = 0;
        double[] m_dlArray = new double[m_BUFFER_ARRAY_SIZE];

        internal int MaxCents
        {
            get { return m_intMaxCents; }
            set { m_intMaxCents = value; }
        }

        private Paint m_pntColor = new Paint
        {
            AntiAlias = true,
            StrokeWidth = 8.0f,
            Color = Color.Rgb(0x99, 0xcc, 0),

            TextSize = 30
        };


        private Paint m_pntGreen = new Paint
        {
            AntiAlias = true,
            StrokeWidth = 8.0f,
            Color = Color.Rgb(0x99, 0xcc, 0),

            TextSize = 30
        };

        private Paint m_pntYellow = new Paint
        {
            AntiAlias = true,
            StrokeWidth = 8.0f,
            Color = Color.Rgb(0xFF, 0xFF, 0),

            TextSize = 30
        };

        public TunerScalesView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {

            Initialize(context);
        }

        public TunerScalesView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {

            Initialize(context);
        }

        private void Initialize(Context context)
        {
            m_Context = context;

            m_pntColor.SetStyle(Style.Fill);
        }

        protected override void OnDraw(Canvas canvas)
        {
            float flCentsClosenessAvg;

            try
            {
                base.OnDraw(canvas);
                //drawSmallCircles(canvas);
                
                flCentsClosenessAvg = (float)GetAvgValueOfBuffer();
                SetColorOfScales(flCentsClosenessAvg);

                UpdateCentsClosenessArray(CentsDeviation);

                DrawTunerScales(canvas);

                DrawPitchOffsetRectangle(canvas);
            }
            catch (Exception e1)
            {
                Toast.MakeText(m_Context, "Exception drawing", ToastLength.Long).Show();
            }
        }

        private void DrawPitchOffsetRectangle(Canvas canvas)
        {
            float flLenghtHorizontaloFscreenToDraw;
            float x1, y1, x2, y2;
            float flHeightOfRectangle = Height / 2.3f;
            float flPercentOFfset, flCentsClosenessAvg;

            
            // this smoothes out the pitch detected
            flCentsClosenessAvg = (float)GetAvgValueOfBuffer();

            //   flPercentOFfset  = (float)flCentsClosenessAvg - 100 ;

            flLenghtHorizontaloFscreenToDraw = (flCentsClosenessAvg / m_intMaxCents) * (Width / 2f);

            //  canvas.drawRect(left, top, right, bottom, paint);

            //  In this :
            //  left: distance of the left side of rectangular from left side of canvas.
            //  top:  Distance of the top side of rectangle from top side of canvas.
            //  right: distance of the right side of rectangular from left side of canvas.
            //  bottom:  Distance of bottom side of rectangular from the top side of canvas
            //
            // or think of it like this:
            // top left of canvas is (0,0)
            // all coords are positive and going away from this
            // x1, y1 is the coords of the top left corner of rect
            // x2, y2 is the coords of the bottom right coords of rect

            // canvas.DrawRect(x1, y1, x2, y2, m_pntGreen);


            if (flCentsClosenessAvg > 0)
            {
                x1 = Width / 2f;
                y1 = Height / 2f - Height / 4f;
                x2 = Width / 2f + flLenghtHorizontaloFscreenToDraw;
                y2 = Height / 2f + Height / 4f;
            }
            else
            {
                x1 = Width / 2f + flLenghtHorizontaloFscreenToDraw;
                y1 = Height / 2f - Height / 4f;
                x2 = Width / 2f;
                y2 = Height / 2f + Height / 4f;
            }
            canvas.DrawRect(x1, y1, x2, y2, m_pntColor);
        }


        private void UpdateCentsClosenessArray(double centsDeviation)
        {
            m_dlArray[intCurrentArrayIndex] = centsDeviation;

            intCurrentArrayIndex++;

            if (intCurrentArrayIndex > m_BUFFER_ARRAY_SIZE - 1)
            {
                intCurrentArrayIndex = 0;
            }
        }

        internal void Destroy()
        {
            base.Dispose();
            this.Dispose();
        }

        internal double GetAvgValueOfBuffer()
        {
            double dbTotal = 0;
            for (int i = 0; i < m_BUFFER_ARRAY_SIZE; i++)
            {
                dbTotal += m_dlArray[i];
            }
            return dbTotal / m_BUFFER_ARRAY_SIZE;
        }

        private void DrawTunerScales(Canvas canvas)
        {
            int intOffsetFromViewEdge = 10;
            int intTextHorizOffsetFromMarker = Width / 50;
            int intTextVertialOffsetFromViewEdge = Height / 8;

            float flTextWidth, flCentsClosenessAvg;

            // this smoothes out the pitch detected
            flCentsClosenessAvg = (float)GetAvgValueOfBuffer();
            
            //hoziontal axis
            canvas.DrawLine(0, Height / 2, Width, Height / 2, m_pntColor);

            // -max int marker
            canvas.DrawLine(intOffsetFromViewEdge, 0 + intOffsetFromViewEdge, intOffsetFromViewEdge, Height - intOffsetFromViewEdge, m_pntColor);
            canvas.DrawText("-" + m_intMaxCents, intOffsetFromViewEdge + intTextHorizOffsetFromMarker, Height - intTextVertialOffsetFromViewEdge, m_pntColor);
            // canvas.DrawText()

            // - middle marker
            canvas.DrawLine(Width / 4, 0 + intOffsetFromViewEdge, Width / 4, Height - intOffsetFromViewEdge, m_pntColor);
            canvas.DrawText("-" + (m_intMaxCents / 2), (Width / 4) + intTextHorizOffsetFromMarker, Height - intTextVertialOffsetFromViewEdge, m_pntColor);

            // 0 marker
            canvas.DrawLine(Width / 2, 0 + intOffsetFromViewEdge, Width / 2, Height - intOffsetFromViewEdge, m_pntColor);
            canvas.DrawText("0", (Width / 2) + intTextHorizOffsetFromMarker, Height - intTextVertialOffsetFromViewEdge, m_pntColor);

            // + middle  marker
            canvas.DrawLine(3 * (Width / 4), 0 + intOffsetFromViewEdge, 3 * (Width / 4), Height - intOffsetFromViewEdge, m_pntColor);

            flTextWidth = m_pntColor.MeasureText("+" + (m_intMaxCents / 2));
            canvas.DrawText("+" + (m_intMaxCents / 2), (3 * (Width / 4)) - (intTextHorizOffsetFromMarker + flTextWidth), Height - intTextVertialOffsetFromViewEdge, m_pntColor);

            // + max int marker
            canvas.DrawLine(Width - intOffsetFromViewEdge, 0 + intOffsetFromViewEdge, Width - intOffsetFromViewEdge, Height - intOffsetFromViewEdge, m_pntColor);
            flTextWidth = m_pntColor.MeasureText("+" + m_intMaxCents);
            canvas.DrawText("+" + m_intMaxCents, (Width - intOffsetFromViewEdge) - (intTextHorizOffsetFromMarker + flTextWidth), Height - intTextVertialOffsetFromViewEdge, m_pntColor);
        }

        private void SetColorOfScales(float flCentsClosenessAvg)
        {
            if (Math.Abs(flCentsClosenessAvg) < 25)
            {
                m_pntColor.Color = Color.Rgb(0x99, 0xcc, 0);
            }

            else
            {
                m_pntColor.Color = Color.Rgb(0xFF, 0xFF, 0x99);
            }
        }

        private void drawSmallCircles(Canvas canvas)
        {
            const int NUM_BUBBLES = 5;
            int radius = 60;

            int spacing = Width / NUM_BUBBLES;
            int shift = spacing / 2;
            int bottomMargin = 10;

            var paintCircle = new Paint() { Color = Color.White };
            for (int i = 0; i < NUM_BUBBLES; i++)
            {
                int x = i * spacing + shift;
                int y = Height - radius * 2 - bottomMargin;
                canvas.DrawCircle(x, y, radius, paintCircle);
            }
        }
    }
}