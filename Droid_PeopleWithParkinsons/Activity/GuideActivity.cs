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
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Guide Activity")]
    public class GuideActivity : FragmentActivity
    {
        GuideAdapter adapter;
        ViewPager pager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.GuideActivity);
            ActionBar.Hide();

            adapter = new GuideAdapter(SupportFragmentManager);
            pager = FindViewById<ViewPager>(Resource.Id.guide_pager);
            pager.Adapter = adapter;
            pager.SetPageTransformer(true, new DepthPageTransformer());
        }
    }

        public class GuideAdapter : FragmentStatePagerAdapter
        {
            private Android.Support.V4.App.FragmentManager SupportFragmentManager;

            public GuideAdapter(Android.Support.V4.App.FragmentManager SupportFragmentManager)
                : base(SupportFragmentManager)
            {
                this.SupportFragmentManager = SupportFragmentManager;
                _count = 5;
            }

            protected internal readonly string[] _titles;

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                Android.Support.V4.App.Fragment fragment = new GuideFragment();
                Bundle args = new Bundle();
                args.PutString("content","Testing!");
                args.PutBoolean("first", position == 0);
                args.PutBoolean("last", position == _count - 1);
                fragment.Arguments = args;
                return fragment;
            }

            private int _count;
            public override int Count
            {
                get { return _count; }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(_titles[position]);
            }

            public void SetCount(int count)
            {
                _count = count;
                NotifyDataSetChanged();
            }
        }

        public class DepthPageTransformer : Java.Lang.Object, ViewPager.IPageTransformer
        {
            private static float MIN_SCALE = 0.75f;

            public void TransformPage(View view, float position)
            {
                int pageWidth = view.Width;

                if(position < -1 )
                {
                    // page is off to the left
                    view.Alpha = 0;
                }
                else if(position <= 0)
                {
                    view.Alpha = 1;
                    view.TranslationX = 0;
                    view.ScaleX = 1;
                    view.ScaleY = 1;
                }
                else if(position <= 1)
                {
                    view.Alpha = 1 - position;
                    view.TranslationX = pageWidth * -position;

                    float scaleFactor = MIN_SCALE + (1 - MIN_SCALE) * (1 - Math.Abs(position));
                    view.ScaleX = scaleFactor;
                    view.ScaleY = scaleFactor;
                }
                else
                {
                    view.Alpha = 0;
                }
            }
        }
    }
