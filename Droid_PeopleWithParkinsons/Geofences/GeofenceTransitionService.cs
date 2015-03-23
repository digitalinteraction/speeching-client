using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Widget;

namespace Droid_PeopleWithParkinsons
{
    [Service]
    public abstract class GeofenceTransitionService : IntentService
    {
        public GeofenceTransitionService() : base()
        {

        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Service requested to start!", ToastLength.Long).Show();
            return base.OnStartCommand(intent, flags, startId);
        }

        protected override void OnHandleIntent(Intent intent)
        {
            GeofencingEvent thisEvent = GeofencingEvent.FromIntent(intent);
            if(intent != null)
            {
                if(thisEvent.HasError)
                {
                    OnError(thisEvent.ErrorCode);
                }
                else
                {
                    int transition = thisEvent.GeofenceTransition;
                    Toast.MakeText(this, "Intent type=" + transition, ToastLength.Long).Show(); 
                    if(transition == Geofence.GeofenceTransitionEnter || transition == Geofence.GeofenceTransitionDwell || transition == Geofence.GeofenceTransitionExit)
                    {
                        string[] geofenceIds = new string[thisEvent.TriggeringGeofences.Count];

                        for(int i = 0; i < geofenceIds.Length; i++)
                        {
                            geofenceIds[i] = thisEvent.TriggeringGeofences[i].RequestId;
                        }

                        if(transition == Geofence.GeofenceTransitionEnter || transition == Geofence.GeofenceTransitionDwell)
                        {
                            OnEnteredGeofences(geofenceIds);
                        }
                        else if(transition == Geofence.GeofenceTransitionExit)
                        {
                            OnExitedGeofences(geofenceIds);
                        }
                    }
                }
            }
        }

        protected abstract void OnEnteredGeofences(string[] geofenceIds);

        protected abstract void OnExitedGeofences(string[] geofenceIds);

        protected abstract void OnError(int errorCode);
    }
}