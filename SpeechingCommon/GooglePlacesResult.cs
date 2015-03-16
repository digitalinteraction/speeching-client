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
    public class GooglePlace
    {
        public struct GeometryData
        {
            public struct loc
            {
                public double lat;
                public double lng;
            }

            public loc location;
        }

        public struct PhotoData
        {
            public string height;
            public string width;
            public string photo_reference;
        }

        public GeometryData geometry;
        public string icon;
        public string id;
        public string place_id;
        public string name;
        public PhotoData[] photos;
        public string[] types;
        public string vicinity;
    }

    public struct PlacesQueryResult
    {
        public GooglePlace[] results;
        public string next_page_token;
    }
}