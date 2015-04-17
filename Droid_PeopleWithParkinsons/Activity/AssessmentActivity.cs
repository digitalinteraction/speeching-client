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

namespace DroidSpeeching
{
    [Activity(Label = "Assessment Activity", ParentActivity = typeof(MainActivity))]
    public class AssessmentActivity : ActionBarActivity
    {
        Button recButton;
        QuickFireFragment fragment;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AssessmentActivity);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            recButton = FindViewById<Button>(Resource.Id.assessment_startBtn);
            recButton.Click += recButton_Click;

            fragment = new QuickFireFragment(new string[]{ "first", "second", "third", "last" });

            FragmentManager.BeginTransaction().Add(Resource.Id.fragment_container, fragment).Commit();
        }

        void recButton_Click(object sender, EventArgs e)
        {
            if(!fragment.finished)
            {
                fragment.ShowNextWord();
            }
            else
            {
                FragmentManager.BeginTransaction().Remove(fragment).Commit();
            }
        }
    }
}