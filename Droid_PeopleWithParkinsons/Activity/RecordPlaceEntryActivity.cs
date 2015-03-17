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
using SpeechingCommon;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Make a new recording")]
    public class RecordPlaceEntryActivity : Activity
    {
        string placeId;
        string placeName;
        string imageLoc;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            placeId = Intent.GetStringExtra("PlaceID");
            placeName = Intent.GetStringExtra("PlaceName");
            imageLoc = Intent.GetStringExtra("PlaceImage");

            SetContentView(Resource.Layout.PlacesRecordEntry);

            if(imageLoc != null)
            {
                FindViewById<ImageView>(Resource.Id.placesRecord_photo).SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));
            }
            else
            {
                // TODO do something which is less disappointing, because the image does look pretty nice
                FindViewById<ImageView>(Resource.Id.placesRecord_photo).Visibility = ViewStates.Gone;
            }
            FindViewById<TextView>(Resource.Id.placesRecord_title).Text = "Create an entry about " + placeName;

        }
    }
}