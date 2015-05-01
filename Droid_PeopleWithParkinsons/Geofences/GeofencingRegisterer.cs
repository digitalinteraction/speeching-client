using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingShared;
using System;
using System.Collections.Generic;

namespace DroidSpeeching
{
    public class GeofencingRegisterer : Java.Lang.Object, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
    {
        private enum FencingStage { Connecting, Removing, Adding, Ready };
        private FencingStage currentStage;

        private Context context;
        private static IGoogleApiClient googleApiClient;
        private List<IGeofence> fencesToAdd;
        private Intent intent;
        private PendingIntent pendingIntent;

        private Action CallbackOnConnected;
        private Action CallbackOnFencesAdded;

        public GeofencingRegisterer(Context context, Action ConnectedCallback, Action FencesAddedCallback)
        {
            this.context = context;
            this.CallbackOnConnected = ConnectedCallback;
            this.CallbackOnFencesAdded = FencesAddedCallback;
        }

        public void RegisterGeofences(PlaceGeofence[] fenceData)
        {
            if(pendingIntent != null)
            {
                Toast.MakeText(context, "Removing old fences", ToastLength.Short).Show();
                currentStage = FencingStage.Removing;
            }

            if (fencesToAdd == null)
            {
                fencesToAdd = new List<IGeofence>();
            }

            ISharedPreferences prefs = context.GetSharedPreferences("FENCES", FileCreationMode.MultiProcess);
            ISharedPreferencesEditor editor = prefs.Edit();

            editor.Clear();
            editor.Apply();

            foreach (PlaceGeofence fence in fenceData)
            {
                fencesToAdd.Add(new GeofenceBuilder()
                    .SetCircularRegion(fence.lat, fence.lng, fence.radius)
                    .SetExpirationDuration(Geofence.NeverExpire)
                    .SetRequestId(fence.placeId)
                    .SetLoiteringDelay(0)
                    .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionDwell | Geofence.GeofenceTransitionExit)
                    .Build());

                editor.PutString(fence.placeId, JsonConvert.SerializeObject(fence));
            }

            editor.Apply();

            if(googleApiClient == null)
            {
                intent = new Intent(context, typeof(GeofencingReceiver));
                googleApiClient = new GoogleApiClientBuilder(context)
                    .AddApi(LocationServices.Api)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .Build();
            }
            
            googleApiClient.Connect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            CallbackOnConnected();

            if(currentStage == FencingStage.Removing)
            {
                LocationServices.GeofencingApi.RemoveGeofences(googleApiClient, pendingIntent);
            }
            else
            {
                AddFences();
            }
        }

        private void AddFences()
        {
            pendingIntent = CreatePendingIntent();

            GeofencingRequest fenceReq = new GeofencingRequest.Builder().AddGeofences(fencesToAdd).Build();

            currentStage = FencingStage.Adding;
            var result = LocationServices.GeofencingApi.AddGeofences(googleApiClient, fenceReq, pendingIntent);
            result.SetResultCallback(this);
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        private PendingIntent CreatePendingIntent()
        {
            if(pendingIntent != null)
            {
                return pendingIntent;
            }
            else
            {
                return PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            }
        }

        public void OnResult(Java.Lang.Object result)
        {
            switch(currentStage)
            {
                case FencingStage.Adding:
                    CallbackOnFencesAdded();
                    break;
                case FencingStage.Removing:
                    AddFences();
                    break;
            }
        }
    }

}