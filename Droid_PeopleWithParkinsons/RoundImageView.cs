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
    public class RoundImageView : ImageView
    {
        public RoundImageView(Context context) 
            : base(context)
        {

        }

        public RoundImageView(Context context, Android.Util.IAttributeSet attrs)
            : base(context, attrs)
        {

        }

        public RoundImageView(Context context, Android.Util.IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            if (Drawable == null)
            {
                return;
            }

            if (Width == 0 || Height == 0)
            {
                return;
            }

            Bitmap b = ((BitmapDrawable)Drawable).Bitmap;
            Bitmap bitmap = b.Copy(Bitmap.Config.Argb8888, true);

            int w = Width, h = Height;


            Bitmap roundBitmap = GetCroppedBitmap(bitmap, w);
            canvas.DrawBitmap(roundBitmap, 0, 0, null);
        }

        public static Bitmap GetCroppedBitmap(Bitmap bmp, int radius)
        {
            Bitmap sbmp;

            if(bmp.Width != radius || bmp.Height != radius)
            {
                float smallest = Math.Min(bmp.Width, bmp.Height);
                float factor = smallest / radius;
                sbmp = Bitmap.CreateScaledBitmap(bmp, (int)(bmp.Width / factor), (int)(bmp.Height / factor), false);
            }   
            else
            {
                sbmp = bmp;
            }
                
            Bitmap output = Bitmap.CreateBitmap(radius, radius, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(output);

            Paint paint = new Paint();
            Rect rect = new Rect(0, 0, radius, radius);

            paint.AntiAlias = true;
            paint.FilterBitmap = true;
            paint.Dither = true;
            canvas.DrawARGB(0, 0, 0, 0);
            paint.Color = Color.ParseColor("#BAB399");
            canvas.DrawCircle(radius / 2+0.7f, radius / 2+0.7f, radius / 2+0.1f, paint);

            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(sbmp, rect, rect, paint);

            return output;
        }
    }
}