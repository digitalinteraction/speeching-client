using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Widget;
using Android.Views;

namespace Droid_PeopleWithParkinsons
{
    class PlaceholderFragment : Android.App.Fragment
    {
        private View ourView;


        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            ourView = inflater.Inflate(Resource.Layout.Main, container, false);

            return ourView;
        }
    }
}
