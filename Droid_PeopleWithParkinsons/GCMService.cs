using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Gcm;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingCommon;
using System;
using System.Collections.Generic;

namespace Droid_PeopleWithParkinsons
{
    public class GcmBroadcastReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                intent.SetClass(context, typeof(GcmIntentService));
                StartWakefulService(context, intent);
            }
            catch(Exception except)
            {
                throw except;
            }
        }
    }

    [Service]
    public class GcmIntentService : IntentService, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
    {
        Intent lastIntent;
        List<IGeofence> fences;

        string lastType = "";
        string lastData = "";
 
        IGoogleApiClient apiClient;
        GeofencingRegisterer fenceReg;

        public GcmIntentService() : base()
        {
            Action onConnected = () => { Toast.MakeText(this, "Connected", ToastLength.Short).Show(); };
            Action onAdded = () => { Toast.MakeText(this, "Fence added", ToastLength.Short).Show(); };
            fenceReg = new GeofencingRegisterer(this, onConnected, onAdded);
        }

        protected override void OnHandleIntent(Intent intent)
        {
            lastIntent = intent;
            Bundle extras = intent.Extras;
            GoogleCloudMessaging gcm = GoogleCloudMessaging.GetInstance(this);
            string messageType = gcm.GetMessageType(intent);


            if(!extras.IsEmpty)
            {
                if(GoogleCloudMessaging.MessageTypeSendError.Equals(messageType))
                {
                    AndroidUtils.SendNotification("Speeching Error", "Error while sending message: " + extras.ToString(), typeof(MainActivity), this);
                }
                else if(GoogleCloudMessaging.MessageTypeDeleted.Equals(messageType))
                {
                    AndroidUtils.SendNotification("Speeching Messages", "Deleted messages on the server", typeof(MainActivity), this);
                }
                else if(GoogleCloudMessaging.MessageTypeMessage.Equals(messageType))
                {
                    string notifType = extras.GetString("notifType");
                    lastType = notifType;
                    PrepClient();

                    switch (notifType)
                    {
                        case "reminder" :
                            ShowReminder();
                            break;
                        case "fences" :
                            BuildFences(extras.GetString("fences"));
                            break;
                    }
                   
                }
            }
        }

        private IGoogleApiClient PrepClient()
        {
            if (apiClient == null)
            {
                GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.Api);

                apiClient = builder.Build();
            }

            return apiClient;
        }

        private void BuildFences(string fencesJson)
        {
            PlaceGeofence[] fenceData = JsonConvert.DeserializeObject<PlaceGeofence[]>(fencesJson);

            fenceReg.RegisterGeofences(fenceData);
        }

        private void ShowReminder()
        {
            if (apiClient.IsConnected)
            {
                Location lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);

                if (lastLoc != null)
                {
                    ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 500, OnPlacesReturned);
                }
            }
            else
            {
                apiClient.Connect();
            }
        }

        public void OnPlacesReturned(GooglePlace[] places)
        {
            if(places.Length > 0)
            {
                string title = "Make a new voice recording!";
                string message = "It looks like you're near " + places[0].name;

                if (places.Length > 1) message += " and other places, such as " + places[1].name;

                message += "! Why not practice your speech by making a voice entry about a nearby location?";

                AndroidUtils.SendNotification(title, message, typeof(LocationActivity), this);

                GcmBroadcastReceiver.CompleteWakefulIntent(lastIntent);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            switch(lastType)
            {
                case "reminder" :
                    Location lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);
                    if (lastLoc != null)
                    {
                        ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 500, OnPlacesReturned);
                    }
                    break;
            }
            
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            throw new NotImplementedException();
        }
    }
}