using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Droid_PeopleWithParkinsons
{
    public class GuideFragment : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.GuideFragment, container, false);
            Bundle args = this.Arguments;

            view.FindViewById<TextView>(Resource.Id.guide_content).Text = args.GetString("content");

            ImageView bg = view.FindViewById<ImageView>(Resource.Id.guide_mainImage);
            bg.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(args.GetString("image"))));

            view.FindViewById<LinearLayout>(Resource.Id.guide_left).Visibility = (args.GetBoolean("first")) ? ViewStates.Gone : ViewStates.Visible;
            view.FindViewById<LinearLayout>(Resource.Id.guide_right).Visibility = (args.GetBoolean("last")) ? ViewStates.Gone : ViewStates.Visible;
            return view;
        }
    }
}