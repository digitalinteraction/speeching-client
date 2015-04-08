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
using SpeechingCommon;

namespace DroidSpeeching
{
    public class SettingsFragment : PreferenceFragment
    {
        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.AddPreferencesFromResource(Resource.Xml.user_settings);

            Preference button = FindPreference("revokeGooglePermissions");
            button.PreferenceClick += button_PreferenceClick;
        }

        private void button_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            AlertDialog alert = new AlertDialog.Builder(Activity)
                .SetTitle("Revoke access to your Google account?")
                .SetMessage("This will result in the deletion of some of your user data. Are you really sure?")
                .SetNegativeButton("Cancel", (arg1, arg2) => { })
                .SetPositiveButton("Delete Data", (arg1, arg2) =>
                {
                    AppData.session = null;
                    AppData.SaveCurrentData();

                    Intent intent = new Intent(Activity, typeof(LoginActivity));
                    intent.PutExtra("revokeGoogle", true);
                    StartActivity(intent);

                    Activity.Finish();
                })
                .Create();
            alert.Show();
        }
    }
}