using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Widget;
using System;

namespace Droid_PeopleWithParkinsons
{
    class GeofencingReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var serviceIntent = new Intent(context, typeof(GeofencingService));
            serviceIntent.PutExtras(intent);
            context.StartService(serviceIntent);
        }
    }



    [Service(Exported=true)]
    [IntentFilter(new String[] { "com.speeching.droid_peoplewithparkinsons.GeofencingService" })]
    public class GeofencingService : IntentService
    {
        int count = 0;

        public GeofencingService()
            : base("com.speeching.droid_peoplewithparkinsons.GeofencingService")
        {
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

                    count++;

                    AndroidUtils.SendNotification("Intent type " + transition, count.ToString() + " intents received", typeof(LocationActivity), this);

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

        protected void OnEnteredGeofences(string[] geofenceIds)
        {
            AndroidUtils.SendNotification("Entered geofence!", "You have entered the geofence. Grats!", typeof(LocationActivity), this);
        }

        protected void OnExitedGeofences(string[] geofenceIds)
        {
            AndroidUtils.SendNotification("Left geofence!", "You have exited the geofence. Grats!", typeof(LocationActivity), this);
        }

        protected void OnError(int errorCode)
        {
            AndroidUtils.SendNotification("Geofence error!", "You broke the system! Grats! Err: " + errorCode, typeof(LocationActivity), this);
        }
    }
}