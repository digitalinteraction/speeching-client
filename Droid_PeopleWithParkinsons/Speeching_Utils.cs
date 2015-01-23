using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Text;
using Android.Graphics;

namespace Droid_PeopleWithParkinsons
{
    static class Speeching_Utils
    {
        public enum DISPLAY_UNIT { DP, PX }


        /// <summary>
        /// Calculates the largest value for text to fit within the supplied bounds in pixels
        /// </summary>
        /// /// <param name="text">Text to fit within the bounds</param>
        /// <param name="startingTextSize">Max text size</param>
        /// <param name="maxHeight">Pixel height of your target bounds</param>
        /// <param name="maxWidth">Pixel width of your target bounds</param>
        /// <param name="returnType">Unit the returned int represents</param>
        /// /// <param name="activity">Current active activity</param>
        /// <returns></returns>
        public static int GenerateTextSize(string text, int startingTextSize, int maxHeight, int maxWidth, DISPLAY_UNIT returnType, Activity activity)
        {
            int textSize = startingTextSize;
            int height = maxHeight;
            while (GetHeightOfMultiLineText(text, textSize, maxWidth) >= maxHeight)
            {
                textSize--;
            }

            switch(returnType)
            {
                case DISPLAY_UNIT.DP:
                    Android.Util.DisplayMetrics m = new Android.Util.DisplayMetrics();
                    activity.WindowManager.DefaultDisplay.GetMetrics(m);
                    int toReturn = (int) (textSize / (m.ScaledDensity * 1.2f));
                    return toReturn;
                case DISPLAY_UNIT.PX:
                    return textSize;
                default:
                    return textSize;
            }
        }


        private static int GetHeightOfMultiLineText(string text, int textSize, int maxWidth)
        {
            TextPaint paint = new TextPaint();
            paint.TextSize = textSize;

            int index = 0;
            int lineCount = 0;

            while (index < text.Length)
            {
                index += paint.BreakText(text, index, text.Length, true, maxWidth, null);
                lineCount++;
            }

            Rect bounds = new Rect();
            paint.GetTextBounds("Yy", 0, 2, bounds);

            // obtain space between lines
            double lineSpacing = Math.Max(0, ((lineCount) * bounds.Height() * 0.25));

            return (int)Math.Floor(lineSpacing + lineCount * bounds.Height());
        }
    }
}
