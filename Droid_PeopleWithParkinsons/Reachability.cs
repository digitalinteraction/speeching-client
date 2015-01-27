using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Net;

namespace Droid_PeopleWithParkinsons
{
    static class Reachability
    {
        // Here we could also include whether or not we are connected to the server etc
        public static bool HasNetworkConnection()
        {
            Android.Content.Context mContext = Android.App.Application.Context;

            if (mContext != null)
            {
                var connectivityManager = (ConnectivityManager) mContext.GetSystemService(Activity.ConnectivityService);
                var activeConnection = connectivityManager.ActiveNetworkInfo;

                if ((activeConnection != null) && activeConnection.IsConnected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
