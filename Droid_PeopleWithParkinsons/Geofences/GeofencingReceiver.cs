using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingCommon;
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
        ISharedPreferences prefs;

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

                    if(transition == Geofence.GeofenceTransitionEnter || transition == Geofence.GeofenceTransitionExit)
                    {
                        PlaceGeofence fence = GetFenceObj(thisEvent.TriggeringGeofences[0].RequestId);

                        if (fence != null)
                        {
                            if (transition == Geofence.GeofenceTransitionEnter)
                            {
                                OnEnteredGeofences(fence);
                            }
                            else
                            {
                                OnExitedGeofences(fence);
                            }
                        }
                    }
                }
            }
        }

        private PlaceGeofence GetFenceObj(string placeId)
        {
            if(prefs == null)
            {
                prefs = GetSharedPreferences("FENCES", FileCreationMode.MultiProcess);
            }

            string json = prefs.GetString(placeId, "");

            if(!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<PlaceGeofence>(json);
            }
            else
            {
                return null;
            }
        }

        private void OnEnteredGeofences(PlaceGeofence fence)
        {
            AndroidUtils.SendNotification("Record about " + fence.name + "!", "It looks like you're near " + fence.name + "! Why not practice your speech by making a voice diary about it?", typeof(LocationActivity), this);
        }

        private void OnExitedGeofences(PlaceGeofence fence)
        {
            AndroidUtils.SendNotification("Record about " + fence.name + "!", "It looks like you're leaving " + fence.name + "! Why not practice your speech by making a voice diary about your visit?", typeof(LocationActivity), this);
        }

        private void OnError(int errorCode)
        {
            AndroidUtils.SendNotification("Geofence error!", "You broke the system! Grats! Err: " + errorCode, typeof(LocationActivity), this);
        }
    }
}