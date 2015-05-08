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
using Android.Support.V7.Widget;
using Android.Graphics;
using Android.Animation;
using Java.Lang;
using Android.Views.Animations;

//https://raw.githubusercontent.com/brnunes/SwipeableRecyclerView/master/lib/src/main/java/com/github/brnunes/swipeablerecyclerview/SwipeableRecyclerViewTouchListener.java

namespace DroidSpeeching
{
    public class SwipeableRecyclerViewTouchListener : RecyclerView.OnScrollListener, RecyclerView.IOnItemTouchListener, View.IOnTouchListener
    {
        // Cached ViewConfiguration and system-wide constant values
        private int mSlop;
        private int mMinFlingVelocity;
        private int mMaxFlingVelocity;
        private long mAnimationTime;

        // Fixed properties
        private RecyclerView mRecyclerView;
        private ISwipeListener mSwipeListener;
        private int mViewWidth = 1; // 1 and not 0 to prevent dividing by zero

        // Transient properties
        private List<PendingDismissData> mPendingDismisses = new List<PendingDismissData>();
        private int mDismissAnimationRefCount = 0;
        private float mAlpha;
        private float mDownX;
        private float mDownY;
        private bool mSwiping;
        private int mSwipingSlop;
        private VelocityTracker mVelocityTracker;
        private int mDownPosition;
        private int mAnimatingPosition = ListView.InvalidPosition;
        private View mDownView;
        private bool mPaused;
        private float mFinalDelta;
        private Context context;

        /**
         * Constructs a new swipe touch listener for the given {@link android.support.v7.widget.RecyclerView}
         *
         * @param recyclerView The recycler view whose items should be dismissable by swiping.
         * @param listener     The listener for the swipe events.
         */
        public SwipeableRecyclerViewTouchListener(RecyclerView recyclerView, ISwipeListener listener, Context context)
        {
            ViewConfiguration vc = ViewConfiguration.Get(recyclerView.Context);
            mSlop = vc.ScaledTouchSlop;
            mMinFlingVelocity = vc.ScaledMinimumFlingVelocity * 16;
            mMaxFlingVelocity = vc.ScaledMaximumFlingVelocity;
            mAnimationTime = recyclerView.Context.Resources.GetInteger(
                    Android.Resource.Integer.ConfigShortAnimTime);
            mRecyclerView = recyclerView;
            mSwipeListener = listener;
            this.context = context;

            /**
             * This will ensure that this SwipeableRecyclerViewTouchListener is paused during list view scrolling.
             * If a scroll listener is already assigned, the caller should still pass scroll changes through
             * to this listener.
             */
            mRecyclerView.SetOnScrollListener(this);
            mRecyclerView.SetOnTouchListener(this);
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            SetEnabled(newState != RecyclerView.ScrollStateDragging);
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy) { }

        public void SetEnabled(bool enabled)
        {
            mPaused = !enabled;
        }

        public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            return HandleTouchEvent(e);
        }

        public void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            //rv.RequestDisallowInterceptTouchEvent(true);
            HandleTouchEvent(e);
        }

        private bool HandleTouchEvent(MotionEvent motionEvent)
        {
            if (mViewWidth < 2)
            {
                mViewWidth = mRecyclerView.Width;
            }

            switch (motionEvent.ActionMasked)
            {
                case MotionEventActions.Down:
                    {
                        if (mPaused)
                        {
                            break;
                        }

                        // Find the child view that was touched (perform a hit test)
                        Rect rect = new Rect();
                        int childCount = mRecyclerView.ChildCount;
                        int[] listViewCoords = new int[2];
                        mRecyclerView.GetLocationOnScreen(listViewCoords);
                        int x = (int)motionEvent.RawX - listViewCoords[0];
                        int y = (int)motionEvent.RawY - listViewCoords[1];
                        View child;
                        for (int i = 0; i < childCount; i++)
                        {
                            child = mRecyclerView.GetChildAt(i);
                            child.GetHitRect(rect);
                            if (rect.Contains(x, y))
                            {
                                mDownView = child;
                                break;
                            }
                        }

                        if (mDownView != null && mAnimatingPosition != mRecyclerView.GetChildPosition(mDownView))
                        {
                            mAlpha = mDownView.Alpha;
                            mDownX = motionEvent.RawX;
                            mDownY = motionEvent.RawY;
                            mDownPosition = mRecyclerView.GetChildPosition(mDownView);
                            mVelocityTracker = VelocityTracker.Obtain();
                            mVelocityTracker.AddMovement(motionEvent);
                        }
                        break;
                    }

                case MotionEventActions.Cancel:
                    {
                        if (mVelocityTracker == null)
                        {
                            break;
                        }

                        if (mDownView != null && mSwiping)
                        {
                            // cancel
                            mDownView.Animate()
                                    .TranslationX(0)
                                    .Alpha(mAlpha)
                                    .SetDuration(mAnimationTime)
                                    .SetListener(null);
                        }
                        mVelocityTracker.Recycle();
                        mVelocityTracker = null;
                        mDownX = 0;
                        mDownY = 0;
                        mDownView = null;
                        mDownPosition = ListView.InvalidPosition;
                        mSwiping = false;
                        break;
                    }

                case MotionEventActions.Up:
                    {
                        if (mVelocityTracker == null)
                        {
                            break;
                        }

                        mFinalDelta = motionEvent.RawX - mDownX;
                        mVelocityTracker.AddMovement(motionEvent);
                        mVelocityTracker.ComputeCurrentVelocity(1000);
                        float velocityX = mVelocityTracker.XVelocity;
                        float absVelocityX = System.Math.Abs(velocityX);
                        float absVelocityY = System.Math.Abs(mVelocityTracker.YVelocity);
                        bool dismiss = false;
                        bool dismissRight = false;
                        if (System.Math.Abs(mFinalDelta) > mViewWidth / 2 && mSwiping)
                        {
                            dismiss = true;
                            dismissRight = mFinalDelta > 0;
                        }
                        else if (mMinFlingVelocity <= absVelocityX && absVelocityX <= mMaxFlingVelocity
                              && absVelocityY < absVelocityX && mSwiping)
                        {
                            // dismiss only if flinging in the same direction as dragging
                            dismiss = (velocityX < 0) == (mFinalDelta < 0);
                            dismissRight = mVelocityTracker.XVelocity > 0;
                        }
                        if (dismiss && mDownPosition != mAnimatingPosition && mDownPosition != ListView.InvalidPosition && mSwipeListener.CanSwipe(mDownPosition))
                        {
                            // dismiss
                            View downView = mDownView; // mDownView gets null'd before animation ends
                            int downPosition = mDownPosition;
                            ++mDismissAnimationRefCount;
                            mAnimatingPosition = mDownPosition;
                            mDownView.Animate()
                                    .TranslationX(dismissRight ? mViewWidth : -mViewWidth)
                                    .Alpha(0)
                                    .SetDuration(mAnimationTime)
                                    .WithEndAction(new Runnable(() =>
                                    {

                                        PerformDismiss(downView, downPosition);

                                    }));
                        }
                        else
                        {
                            // cancel
                            mDownView.Animate()
                                    .TranslationX(0)
                                    .Alpha(mAlpha)
                                    .SetDuration(mAnimationTime)
                                    .SetListener(null);
                            if(!mSwipeListener.CanSwipe(mDownPosition) && System.Math.Abs(mFinalDelta) > 10)
                            {
                                Animation shake = AnimationUtils.LoadAnimation(context, Resource.Animation.shake);
                                mDownView.StartAnimation(shake);
                                Toast.MakeText(context, "Can't dismiss that!", ToastLength.Short).Show();
                            }
                        }
                        mVelocityTracker.Recycle();
                        mVelocityTracker = null;
                        mDownX = 0;
                        mDownY = 0;
                        mDownView = null;
                        mDownPosition = ListView.InvalidPosition;
                        mSwiping = false;
                        break;
                    }

                case MotionEventActions.Move:
                    {
                        if (mVelocityTracker == null || mPaused)
                        {
                            break;
                        }

                        mVelocityTracker.AddMovement(motionEvent);
                        float deltaX = motionEvent.RawX - mDownX;

                        float deltaY = motionEvent.RawY - mDownY;
                        if (!mSwiping && System.Math.Abs(deltaX) > mSlop && System.Math.Abs(deltaY) < System.Math.Abs(deltaX) / 2)
                        {
                            mSwiping = true;
                            mSwipingSlop = (deltaX > 0 ? mSlop : -mSlop);
                        }

                        if (mSwiping)
                        {
                            mDownView.TranslationX = (deltaX - mSwipingSlop);
                            mDownView.Alpha = (System.Math.Max(0f, System.Math.Min(mAlpha,
                                    mAlpha * (1f - System.Math.Abs(deltaX) / mViewWidth))));
                            return true;
                        }
                        break;
                    }
            }
            return false;
        }

        private void PerformDismiss(View dismissView, int dismissPosition)
        {
            // Animate the dismissed list item to zero-height and fire the dismiss callback when
            // all dismissed list item animations have completed. This triggers layout on each animation
            // frame; in the future we may want to do something smarter and more performant.

            ViewGroup.LayoutParams lp = dismissView.LayoutParameters;
            int originalLayoutParamsHeight = lp.Height;
            int originalHeight = dismissView.Height;

            ValueAnimator animator = ValueAnimator.OfInt(originalHeight, 1);
            animator.SetDuration( mAnimationTime);

            animator.AnimationEnd += (object sender, EventArgs e) =>
            {
                --mDismissAnimationRefCount;

                if (mDismissAnimationRefCount == 0)
                {
                    // No active animations, process all pending dismisses.
                    // Sort by descending position
                    mPendingDismisses.Sort();

                    int[] dismissPositions = new int[mPendingDismisses.Count];
                    for (int i = mPendingDismisses.Count - 1; i >= 0; i--)
                    {
                        dismissPositions[i] = mPendingDismisses[i].position;
                    }

                    if (mFinalDelta > 0)
                    {
                        mSwipeListener.OnDismissedBySwipeRight(mRecyclerView, dismissPositions);
                    }
                    else
                    {
                        mSwipeListener.OnDismissedBySwipeLeft(mRecyclerView, dismissPositions);
                    }

                    // Reset mDownPosition to avoid MotionEvent.ACTION_UP trying to start a dismiss
                    // animation with a stale position
                    mDownPosition = ListView.InvalidPosition;

                    ViewGroup.LayoutParams layoutParams;
                    foreach (PendingDismissData pendingDismiss in mPendingDismisses)
                    {
                        // Reset view presentation
                        pendingDismiss.view.Alpha = mAlpha;
                        pendingDismiss.view.TranslationX = 0;

                        layoutParams = pendingDismiss.view.LayoutParameters;
                        layoutParams.Height = originalLayoutParamsHeight;

                        pendingDismiss.view.LayoutParameters = layoutParams;
                    }

                    // Send a cancel event
                    long time = SystemClock.UptimeMillis();
                    MotionEvent cancelEvent = MotionEvent.Obtain(time, time,
                            MotionEventActions.Cancel, 0, 0, 0);
                    mRecyclerView.DispatchTouchEvent(cancelEvent);

                    mPendingDismisses.Clear();
                    mAnimatingPosition = ListView.InvalidPosition;
                }
            };

            animator.Update += (object sender, ValueAnimator.AnimatorUpdateEventArgs e) => {
                lp.Height = (int)e.Animation.AnimatedValue;
                dismissView.LayoutParameters = lp;
            };

            mPendingDismisses.Add(new PendingDismissData(dismissPosition, dismissView));
            animator.Start();
             
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            float deltaX = e.RawX - mDownX;
            float deltaY = e.RawY - mDownY;

            // If there is allowed movement on the X axis and we aren't swiping vertically
            if (((deltaX > 20 && mSwipeListener.CanSwipeRight) || (deltaX < -20 && mSwipeListener.CanSwipeLeft)) && (System.Math.Abs(deltaY) < System.Math.Abs(deltaX)))
            {
                v.Parent.RequestDisallowInterceptTouchEvent(true); // See override on CustomSwipeToRefresh
                return HandleTouchEvent(e);
            }
            v.Parent.RequestDisallowInterceptTouchEvent(false);
            return false;
        }
    }

    public interface ISwipeListener
    {
        /**
         * Called to determine whether the given position can be swiped.
         */
        bool CanSwipe(int position);

        bool CanSwipeLeft { get; }
        bool CanSwipeRight { get; }

        /**
         * Called when the item has been dismissed by swiping to the left.
         *
         * @param recyclerView           The originating {@link android.support.v7.widget.RecyclerView}.
         * @param reverseSortedPositions An array of positions to dismiss, sorted in descending
         *                               order for convenience.
         */
        void OnDismissedBySwipeLeft(RecyclerView recyclerView, int[] reverseSortedPositions);

        /**
         * Called when the item has been dismissed by swiping to the right.
         *
         * @param recyclerView           The originating {@link android.support.v7.widget.RecyclerView}.
         * @param reverseSortedPositions An array of positions to dismiss, sorted in descending
         *                               order for convenience.
         */
        void OnDismissedBySwipeRight(RecyclerView recyclerView, int[] reverseSortedPositions);
    }

    class PendingDismissData : IComparable<PendingDismissData> 
    {
        public int position;
        public View view;

        public PendingDismissData(int position, View view) {
            this.position = position;
            this.view = view;
        }
        
        public int CompareTo(PendingDismissData other)
        {
 	        return other.position - position;
        }
    }

}