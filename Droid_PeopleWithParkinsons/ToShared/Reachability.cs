using System;
using System.Collections.Generic;
using System.Text;

#if __ANDROID__
using Android.App;
using Android.Net;
#endif

namespace Droid_PeopleWithParkinsons
{
    static class Reachability
    {

#if __ANDROID__
        private static bool HasAndroidConnection()
        {
            Android.Content.Context mContext = Android.App.Application.Context;

            if (mContext != null)
            {
                var connectivityManager = (ConnectivityManager)mContext.GetSystemService(Activity.ConnectivityService);
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
#elif __IOS__
        private static bool HasIOSConnection()
        {
            throw new NotImplementedException();
        }
#endif

        // Here we could also include whether or not we are connected to the server etc
        public static bool HasNetworkConnection()
        {
#if __ANDROID__
            return HasAndroidConnection();
#else
            return HasIOSConnection();
#endif

        }
    }
}
