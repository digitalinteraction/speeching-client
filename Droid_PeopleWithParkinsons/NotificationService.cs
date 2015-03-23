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

        string lastType = "";
        string lastData = "";
        NotificationManager notificationManager;
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
                    SendNotification("Error while sending message: " + extras.ToString(), "Speeching Error");
                }
                else if(GoogleCloudMessaging.MessageTypeDeleted.Equals(messageType))
                {
                    SendNotification("Deleted messages on server: " + extras.ToString(), "Speeching Messages");
                }
                else if(GoogleCloudMessaging.MessageTypeMessage.Equals(messageType))
                {
                    string notifType = extras.GetString("notifType");

                    lastType = notifType;
                    PrepClient();

                    switch (notifType)
                    {
                        case "reminder" :
                            RecordReminder();
                            break;
                        case "poiUpdate" :

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

        private void RecordReminder()
        {
            if (apiClient.IsConnected)
            {
                Location lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);

                if (lastLoc != null)
                {
                    AddFence(lastLoc);
                    ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 500, OnPlacesReturned);
                }
            }
            else
            {
                apiClient.Connect();
            }
        }

        private void SendNotification(string message, string title)
        {
            if (notificationManager == null)
            {
                notificationManager = (NotificationManager) this.GetSystemService(Context.NotificationService);
            }

            Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType( typeof(LocationActivity) ));
            stackBuilder.AddNextIntent(new Intent(this, typeof(LocationActivity)));

            PendingIntent contentIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                .SetPriority(0)
                .SetLights(300, 1000, 1000)
                .SetVisibility(1)
                .SetLocalOnly(false)
                .SetAutoCancel(true)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(message));

            builder.SetContentIntent(contentIntent);

            notificationManager.Notify(8675309, builder.Build());
        }

        public void OnPlacesReturned(GooglePlace[] places)
        {
            if(places.Length > 0)
            {
                string title = "Make a new voice recording!";
                string message = "It looks like you're near " + places[0].name;

                if (places.Length > 1) message += " and other places, such as " + places[1].name;

                message += "! Why not practice your speech by making a voice entry about a nearby location?";

                SendNotification(message, title);

                GcmBroadcastReceiver.CompleteWakefulIntent(lastIntent);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            switch(lastType)
            {
                case "poiUpdate" :

                    break;

                case "reminder" :
                    Location lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);
                    if (lastLoc != null)
                    {
                        AddFence(lastLoc);
                        ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 500, OnPlacesReturned);
                    }
                    break;
            }
            
        }

        public void AddFence(Location loc)
        {
            List<IGeofence> fences = new List<IGeofence>();

            fences.Add(new GeofenceBuilder()
                .SetCircularRegion(loc.Latitude, loc.Longitude, 10)
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetRequestId("myFence")
                .SetLoiteringDelay(1000)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter|Geofence.GeofenceTransitionDwell|Geofence.GeofenceTransitionExit)
                .Build());

            fenceReg.RegisterGeofences(fences);
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