using Android.App;
using Android.Widget;
using System;

namespace Droid_PeopleWithParkinsons
{
    [Service]
    public class GeofencingReceiver : GeofenceTransitionService
    {
        public GeofencingReceiver() : base()
        {

        }

        protected override void OnEnteredGeofences(string[] geofenceIds)
        {
            Toast.MakeText(this, "Entered!", ToastLength.Long).Show();
            StartActivity(typeof(LocationActivity));
        }

        protected override void OnExitedGeofences(string[] geofenceIds)
        {
            Toast.MakeText(this, "Exit!", ToastLength.Long).Show();
            StartActivity(typeof(LocationActivity));
        }

        protected override void OnError(int errorCode)
        {
            throw new NotImplementedException();
        }
    }
}