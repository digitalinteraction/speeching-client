using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using System.Threading.Tasks;
using System.Net.Http;
using SpeechingCommon;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.Gms.Maps.Model;
using Android.Support.V4.App;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "LocationActivity", ParentActivity = typeof(MainActivity))]
    public class LocationActivity : Activity, Android.Gms.Maps.IOnMapReadyCallback, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, GoogleMap.IOnMarkerClickListener
    {
        MapFragment mapFragment;
        GoogleMap map;
        IGoogleApiClient apiClient;
        Location lastLoc;
        GooglePlace[] nearby;
        bool placed = false;

        ListView mainList;
        ProgressBar spinner;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.LocationActivity);
            mapFragment = MapFragment.NewInstance();
            Android.App.FragmentTransaction tx = FragmentManager.BeginTransaction();
            tx.Add(Resource.Id.map_fragment_container, mapFragment);
            tx.Commit();

            spinner = FindViewById<ProgressBar>(Resource.Id.placesProgress);
            spinner.Visibility = ViewStates.Visible;

            mainList = FindViewById<ListView>(Resource.Id.placesList);
            View header = LayoutInflater.Inflate(Resource.Layout.PlacesListHeader, null);
            mainList.AddHeaderView(header, null, false);
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                GooglePlace thisPlace = nearby[args.Position - 1];
                ZoomToLoc(thisPlace.geometry.location.lat, thisPlace.geometry.location.lng, 17);

                AlertDialog alert = new AlertDialog.Builder(this)
                    .SetTitle("Record an entry?")
                    .SetMessage("Would you like to record a new voice entry about " + thisPlace.name + "?")
                    .SetCancelable(true)
                    .SetPositiveButton("Create new entry", (s, a) => { LaunchRecorder(thisPlace); })
                    .SetNegativeButton("Cancel", (s, a) => { })
                    .Create();

                alert.Show();
            };
            mainList.Visibility = ViewStates.Gone;

            nearby = null;
            placed = false;

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            mapFragment.GetMapAsync(this);
            ReadyGoogleApi();
        }

        private async Task LaunchRecorder(GooglePlace place)
        {
            ProgressDialog dialog = ProgressDialog.Show(this, "Please wait", "Readying data...", true);

            Intent intent = new Intent(this, typeof(RecordPlaceEntryActivity));

            string imgRef = (place.photos != null) ? place.photos[0].photo_reference : null;

            if(imgRef != null)
            {
                imgRef = await ServerData.FetchPlacePhoto(place, 800, 600);
            }

            intent.PutExtra("PlaceImage", imgRef);
            intent.PutExtra("PlaceName", place.name);
            intent.PutExtra("PlaceID", place.place_id);
            intent.PutExtra("PlaceLat", place.geometry.location.lat);
            intent.PutExtra("PlaceLng", place.geometry.location.lng);

            dialog.Dismiss();

            StartActivity(intent);
        }

        private void ReadyGoogleApi()
        {
            GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this)
            .AddConnectionCallbacks(this)
            .AddOnConnectionFailedListener(this)
            .AddApi(LocationServices.Api);

            apiClient = builder.Build();
            apiClient.Connect();
        }

        public void OnMapReady(GoogleMap finalMap)
        {
            map = finalMap;

            map.SetOnMarkerClickListener(this);

            if(nearby != null && !placed)
            {
                PopulateMap();
                PopulateList();
            }
        }

        /// <summary>
        /// Zoom the map to focus on this location
        /// </summary>
        /// <param name="loc"></param>
        private void ZoomToLoc(double latitude, double longitude, int zoomLevel)
        {
            if(map == null) return;
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(new LatLng(latitude, longitude));
            builder.Zoom(zoomLevel);

            CameraPosition camPos = builder.Build();
            CameraUpdate camUpdate = CameraUpdateFactory.NewCameraPosition(camPos);
            map.MoveCamera(camUpdate);
        }

        /// <summary>
        /// We've successfully connected to the Google Play API, so we can get the last known location of the device
        /// </summary>
        /// <param name="connectionHint"></param>
        public void OnConnected(Bundle connectionHint)
        {
            lastLoc = LocationServices.FusedLocationApi.GetLastLocation(apiClient);
           
            if(lastLoc != null)
            {
                ServerData.FetchPlaces(lastLoc.Latitude.ToString(), lastLoc.Longitude.ToString(), 600, OnPlacesReturned);
            }
        }

        /// <summary>
        /// Add markers to the map fragment
        /// </summary>
        public void PopulateMap()
        {
            placed = true; // so this doesn't get called twice
            map.Clear();

            if (lastLoc != null)
            {
                // Zoom to the device's location and add a marker on the map
                ZoomToLoc(lastLoc.Latitude, lastLoc.Longitude, 15);
                MarkerOptions marker = new MarkerOptions();
                marker.SetPosition(new LatLng(lastLoc.Latitude, lastLoc.Longitude));
                marker.SetTitle("Your Location");
                marker.InvokeIcon(BitmapDescriptorFactory.DefaultMarker(180f));
                map.AddMarker(marker);
            }

            foreach (GooglePlace place in nearby)
            {
                MarkerOptions marker = new MarkerOptions();
                marker.SetPosition(new LatLng(place.geometry.location.lat, place.geometry.location.lng));
                marker.SetTitle(place.name);
                map.AddMarker(marker);
            }
        }

        /// <summary>
        /// Create the list using the found nearby locations
        /// </summary>
        public async Task PopulateList()
        {
            await AndroidUtils.InitSession(this);
            mainList.Adapter = new PlacesListAdapter(this, Resource.Id.placesList, nearby);
            spinner.Visibility = ViewStates.Gone;
            mainList.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Locations returned from Google Places API call
        /// </summary>
        /// <param name="places"></param>
        public void OnPlacesReturned(GooglePlace[] places)
        {
            nearby = places;

            if(map != null && !placed)
            {
                PopulateMap();
                PopulateList();
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            // TODO
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            //TODO show alert
            throw new NotImplementedException();
        }

        protected override void OnPause()
        {
            base.OnPause();
            AppData.SaveCurrentData();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //As this Activity can be called from notfications, it's possible that the parent activity isn't in the back stack
            //http://developer.android.com/training/implementing-navigation/ancestral.html
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Intent upIntent = NavUtils.GetParentActivityIntent(this);
                    Toast.MakeText(this, upIntent.Component.PackageName + " | " + upIntent.Component.ClassName, ToastLength.Short).Show();
                    if(NavUtils.ShouldUpRecreateTask(this, upIntent))
                    {
                        // This activity is NOT part of this app's task, so create a new task
                        // when navigating up, with a synthesized back stack.
                        Android.Support.V4.App.TaskStackBuilder.Create(this)
                            // Add all of this activity's parents to the back stack
                            .AddNextIntentWithParentStack(upIntent)
                            // Navigate up to the closest parent
                            .StartActivities();
                    }
                    else
                    {
                        // This activity is part of this app's task, so simply
                        // navigate up to the logical parent activity.
                        NavUtils.NavigateUpTo(this, upIntent);
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public class PlacesListAdapter : BaseAdapter<GooglePlace>
        {
            Activity context;
            GooglePlace[] places;

            public PlacesListAdapter(Activity context, int resource, GooglePlace[] data)
            {
                this.context = context;
                this.places = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override GooglePlace this[int position]
            {
                get { return places[position]; }
            }

            public override int Count
            {
                get { return places.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {

                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.PlacesListItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.placesList_name).Text = places[position].name;

                ImageView photoView = view.FindViewById<ImageView>(Resource.Id.placesList_photo);
                photoView.Visibility = ViewStates.Invisible;

                if (places[position].photos != null && places[position].photos.Length > 0)
                {
                    // A photo is available to show!
                    LoadImage(photoView, places[position]);
                }

                return view;
            }

            private async Task LoadImage(ImageView image, GooglePlace place)
            {
                string imageLoc = await ServerData.FetchPlacePhoto(place, 110, 110);

                if(!string.IsNullOrEmpty(imageLoc))
                {
                    image.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));
                    image.Visibility = ViewStates.Visible;
                }
            }
        }

        /// <summary>
        /// When a marker is tapped on the map, scroll the list so that place is in view
        /// </summary>
        public bool OnMarkerClick(Marker marker)
        {
            LatLng pos = marker.Position;

            int foundPos = -1;

            // If the user's current location was tapped, scroll to the top of the list
            if(lastLoc.Latitude == pos.Latitude &&
                lastLoc.Longitude == pos.Longitude)
            {
                mainList.SmoothScrollToPosition(0);
                return false;
            }

            for(int i = 0; i < nearby.Length; i++)
            {
                if(nearby[i].geometry.location.lat == pos.Latitude &&
                    nearby[i].geometry.location.lng == pos.Longitude)
                {
                    foundPos = i + 1; // account for list header
                    break;
                }
            }

            if(foundPos != -1)
            {
                mainList.SmoothScrollToPosition(foundPos);
            }
            else
            {
                Toast.MakeText(this, "Not found", ToastLength.Short).Show();
            }

            return false;
        }

    }
}