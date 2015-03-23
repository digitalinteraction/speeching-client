using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.OS;
using System;
using System.Collections.Generic;

namespace Droid_PeopleWithParkinsons
{
    public class GeofencingRegisterer : Java.Lang.Object, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
    {
        private enum FencingStage { Connecting, Adding, Ready };
        private FencingStage currentStage;

        private Context context;
        private IGoogleApiClient googleApiClient;
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

        public void RegisterGeofences(List<IGeofence> geofences)
        {
            fencesToAdd = geofences;

            googleApiClient = new GoogleApiClientBuilder(context)
                .AddApi(LocationServices.Api)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .Build();
            googleApiClient.Connect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            CallbackOnConnected();

            pendingIntent = CreatePendingIntent();

            context.StartService(intent);

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
                intent = new Intent(context, typeof(GeofencingReceiver));
                return PendingIntent.GetService(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            }
        }

        public void OnResult(Java.Lang.Object result)
        {
            switch(currentStage)
            {
                case FencingStage.Adding:
                    CallbackOnFencesAdded();
                    break;
            }
        }
    }

}