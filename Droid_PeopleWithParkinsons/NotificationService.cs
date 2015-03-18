using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Gcm;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using SpeechingCommon;
using System;

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
        static Intent lastIntent;
        NotificationManager notificationManager;
        IGoogleApiClient apiClient;

        public GcmIntentService() : base()
        {

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
                    bool shouldUseLocation = extras.GetString("notifType") == "location";

                    if(apiClient == null)
                    {
                        GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this)
                        .AddConnectionCallbacks(this)
                        .AddOnConnectionFailedListener(this)
                        .AddApi(LocationServices.Api);

                        apiClient = builder.Build();
                    }
                    if(apiClient.IsConnected)
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
            }
        }

        private void SendNotification(string message, string title)
        {
            if (notificationManager == null)
            {
                notificationManager = (NotificationManager) this.GetSystemService(Context.NotificationService);
            }

            PendingIntent contentIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(LocationActivity)), 0);

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
            Location lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);

            if (lastLoc != null)
            {
                ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 500, OnPlacesReturned);
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