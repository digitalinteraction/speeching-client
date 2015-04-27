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
using Android.Support.V4.Widget;
using Android.Util;

namespace DroidSpeeching
{
    public class CustomSwipeToRefresh : SwipeRefreshLayout
    {
        private int mTouchSlop;
        private float mPrevX;

        public CustomSwipeToRefresh(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            mTouchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            switch (ev.Action) 
            {
                case MotionEventActions.Down:
                    mPrevX = MotionEvent.Obtain(ev).RawX;
                    break;

                case MotionEventActions.Move:
                    float eventX = ev.RawX;
                    float xDiff = Math.Abs(eventX - mPrevX);

                    if (xDiff > mTouchSlop) 
                    {
                        return false;
                    }
                    break;
            }

            return base.OnInterceptTouchEvent(ev);
        }
    }
}