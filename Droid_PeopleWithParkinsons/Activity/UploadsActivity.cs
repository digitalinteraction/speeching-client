using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Uploads", ParentActivity = typeof(MainActivity))]
    public class UploadsActivity : Activity
    {
        private Button uploadAllButton;
        private ListView uploadsList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UploadsActivity);

            uploadsList = FindViewById<ListView>(Resource.Id.uploads_list);
            uploadsList.Adapter = new AndroidUtils.ExportedListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());
            uploadsList.ItemClick += OnItemTap;

            uploadAllButton = FindViewById<Button>(Resource.Id.uploads_start);
        }

        public void OnItemTap(object sender, AdapterView.ItemClickEventArgs args)
        {
            AlertDialog alert = new AlertDialog.Builder(this)
            .SetTitle("Scenario Complete!")
            .SetMessage("What would you like to do with the results of this scenario?")
            .SetCancelable(true)
            .SetNegativeButton("Delete", (EventHandler<DialogClickEventArgs>)null)
            .SetPositiveButton("Upload", (s, a) => { })
            .SetNeutralButton("Cancel", (s, a) => { })
            .Create();

            alert.Show();

            // A second alert dialogue, confirming the decision to delete
            Button negative = alert.GetButton((int)DialogButtonType.Negative);
            negative.Click += delegate(object s, EventArgs e)
            {
                AlertDialog.Builder confirm = new AlertDialog.Builder(this);
                confirm.SetTitle("Are you sure?");
                confirm.SetMessage("The recorded data will be irrecoverably lost.");
                confirm.SetPositiveButton("Delete", (senderAlert, confArgs) =>
                {
                    AppData.session.DeleteResult(AppData.session.resultsToUpload[args.Position]);

                    uploadsList.Adapter = null;
                    uploadsList.Adapter = new AndroidUtils.ExportedListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());

                    alert.Dismiss();
                });
                confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                confirm.Show();
            };     
        }
    }
}