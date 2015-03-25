using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public class GeofencingService : IntentService, IGoogleApiClientConnectionCallbacks
    {
        ISharedPreferences prefs;
        IGoogleApiClient client;
        List<string> toRemove;

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

                    if(transition == Geofence.GeofenceTransitionEnter || transition == Geofence.GeofenceTransitionExit ||  transition == Geofence.GeofenceTransitionDwell)
                    {
                        PlaceGeofence fence = GetFenceObj(thisEvent.TriggeringGeofences[0].RequestId);

                        if (fence != null)
                        {
                            if (transition == Geofence.GeofenceTransitionEnter || transition == Geofence.GeofenceTransitionDwell)
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
            string json = GetPrefs().GetString(placeId, "");

            if(!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<PlaceGeofence>(json);
            }
            else
            {
                return null;
            }
        }

        private async Task<Intent> PrepareIntent(PlaceGeofence fence)
        {
            Intent intent = new Intent(this, typeof(RecordPlaceEntryActivity));

            string imgRef = (fence.imageRef != null) ? fence.imageRef : null;

            if (imgRef != null)
            {
                imgRef = await ServerData.FetchPlacePhoto(fence, 800, 600);
            }

            intent.PutExtra("PlaceImage", imgRef);
            intent.PutExtra("PlaceName", fence.name);
            intent.PutExtra("PlaceID", fence.placeId);
            intent.PutExtra("PlaceLat", fence.lat);
            intent.PutExtra("PlaceLng", fence.lng);

            return intent;
        }

        /// <summary>
        /// Stop watching this fence to save battery/ prevent being annoying
        /// </summary>
        /// <param name="fence"></param>
        private void RemoveFence(PlaceGeofence fence)
        {
            if(client == null)
            {
                client = new GoogleApiClientBuilder(this)
                    .AddApi(LocationServices.Api)
                    .AddConnectionCallbacks(this)
                    .Build();
            }

            client.Connect();

            toRemove = new List<string>();
            toRemove.Add(fence.placeId);
        }

        private async Task OnEnteredGeofences(PlaceGeofence fence)
        {
            Intent intent = await PrepareIntent(fence);
            AndroidUtils.SendNotification(
                "Record about " + fence.name + "!", "It looks like you're near " + fence.name + "! Why not practice your speech by making a voice diary about it?",
                typeof(RecordPlaceEntryActivity), 
                intent,
                this,
                2
            );
            RemoveFence(fence);
        }

        private async Task OnExitedGeofences(PlaceGeofence fence)
        {
            Intent intent = await PrepareIntent(fence);
            AndroidUtils.SendNotification(
                "Record about " + fence.name + "!", "It looks like you're leaving " + fence.name + "! Why not practice your speech by making a voice diary about your visit?",
                typeof(RecordPlaceEntryActivity), 
                intent,
                this
            );
            RemoveFence(fence);
        }

        private void OnError(int errorCode)
        {
            AndroidUtils.SendNotification("Geofence error!", "You broke the system! Grats! Err: " + errorCode, typeof(LocationActivity), this);
        }

        public void OnConnected(Android.OS.Bundle connectionHint)
        {
            LocationServices.GeofencingApi.RemoveGeofences(client, toRemove);
            ISharedPreferencesEditor editor = GetPrefs().Edit();

            foreach (string id in toRemove)
                editor.Remove(id);

            editor.Apply();
        }

        public void OnConnectionSuspended(int cause)
        {
            
        }

        private ISharedPreferences GetPrefs()
        {
            if (prefs == null)
            {
                prefs = GetSharedPreferences("FENCES", FileCreationMode.MultiProcess);
            }

            return prefs;
        }
    }
}