using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace DroidSpeeching
{
    /// <summary>
    /// Creates a circular drawable out of the given Bitmap
    /// Based on: http://evel.io/2013/07/21/rounded-avatars-in-android/
    /// </summary>
    public class RoundedDrawable : Drawable
    {
        private Bitmap mBitmap;
        private Paint mPaint;
        private RectF mRectF;
        private int mBitmapWidth;
        private int mBitmapHeight;

        public RoundedDrawable(Bitmap bitmap) : base()
        {
            mBitmap = bitmap;
            mRectF = new RectF();
            mPaint = new Paint();
            mPaint.AntiAlias = true;
            mPaint.Dither = true;

            BitmapShader shader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            mPaint.SetShader(shader);

            // Try to get it circular, even if the given bitmap isn't square
            mBitmapWidth = Math.Min( mBitmap.Width, mBitmap.Height);
            mBitmapHeight = Math.Min(mBitmap.Width, mBitmap.Height);
        }

        public override void Draw(Canvas canvas)
        {
            canvas.DrawOval(mRectF, mPaint);
        }

        public override int Opacity
        {
            get { return (int)Format.Opaque; }
        }

        public override void SetAlpha(int alpha)
        {
            if (mPaint.Alpha != alpha)
            {
                mPaint.Alpha = alpha;
                InvalidateSelf();
            }
        }

        protected override void OnBoundsChange(Rect bounds)
        {
            base.OnBoundsChange(bounds);
            mRectF.Set(0, 0, bounds.Width(), bounds.Height());
        }

        public override void SetColorFilter(ColorFilter cf)
        {
            mPaint.SetColorFilter(cf);
        }

        public override int IntrinsicHeight
        {
            get
            {
                return mBitmapHeight;
            }
        }

        public override int IntrinsicWidth
        {
            get
            {
                return mBitmapWidth;
            }
        }

        public override void SetFilterBitmap(bool filter)
        {
            mPaint.Dither = filter;
            InvalidateSelf();
        }

        public void SetAntiAlias(bool aa)
        {
            mPaint.AntiAlias = aa;
            InvalidateSelf();
        }

        public override void SetDither(bool dither)
        {
            mPaint.Dither = dither;
            InvalidateSelf();
        }

        public Bitmap GetBitmap()
        {
            return mBitmap;
        }
    }
}