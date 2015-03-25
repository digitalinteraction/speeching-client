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

namespace SpeechingCommon
{
    public class PlaceGeofence
    {
        public string name;
        public int radius;
        public double lat;
        public double lng;
        public string placeId;
        public string imageRef;
    }
}