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
using Android.Gms.Common;

namespace Droid_PeopleWithParkinsons
{
    //private GoogleApiClient client;

    public class LocationFragment : Android.Support.V4.App.Fragment//, IGooglePlayServicesClientOnConnectionFailedListener
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.LocationFragment, container, false);
            return view;
        }

        public void OnConnected(Bundle bundle)
        {

        }

        public void OnDisconnected(Bundle bundle)
        {

        }

        public void OnConnectionFailed(Bundle bundle)
        {

        }
    }
}