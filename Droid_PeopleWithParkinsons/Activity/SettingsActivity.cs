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
using Android.Support.V7.App;
using Android.Support.V4.App;

namespace DroidSpeeching
{
    /// <summary>
    /// Wrap settings in an Activity to allow for actionbar up navigation
    /// </summary>
    [Activity(Label = "Settings", ParentActivity = typeof(MainActivity))]
    public class SettingsActivity : ActionBarActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new SettingsFragment()).Commit();
        }

        // For the home button in top left
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}