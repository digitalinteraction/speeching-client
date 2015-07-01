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

namespace Droid_Dysfluency
{
    public class CustomSwipeToRefresh : SwipeRefreshLayout
    {
        private int mTouchSlop;
        private float mPrevX;
        private bool declined = false;
        bool allowed = true;

        public CustomSwipeToRefresh(IntPtr pointer, JniHandleOwnership jni) : base(pointer, jni)
        {

        }

        public CustomSwipeToRefresh(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            mTouchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
        }

        public void SetSlop(Context context)
        {
            mTouchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;
        }

        // This has been disabled in the base class for some stupid, unexplained reason.
        public override void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            if(disallowIntercept == !allowed)
            {
                return;
            }

            allowed = !disallowIntercept;

            if(this.Parent != null)
            {
                Parent.RequestDisallowInterceptTouchEvent(disallowIntercept);
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (!allowed) return false;

            switch (ev.Action) 
            {
                case MotionEventActions.Down:
                    mPrevX = MotionEvent.Obtain(ev).RawX;
                    declined = false;
                    break;

                case MotionEventActions.Move:
                    float eventX = ev.RawX;
                    float xDiff = Math.Abs(eventX - mPrevX);

                    if (declined || xDiff > mTouchSlop) 
                    {
                        declined = true;
                        return false;
                    }
                    break;
            }

            return base.OnInterceptTouchEvent(ev);
        }
    }
}