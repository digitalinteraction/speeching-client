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
using Android.Preferences;

namespace DroidSpeeching
{
    public class SettingsFragment : PreferenceFragment
    {
        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.AddPreferencesFromResource(Resource.Xml.user_settings);
        }
    }
}