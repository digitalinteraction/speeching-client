using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System;

namespace DroidSpeeching
{
    [Activity(Label = "Uploads", ParentActivity = typeof(MainActivity))]
    public class UploadsActivity : ActionBarActivity
    {
        private ToggleButton uploadAllButton;
        private ListView uploadsList;
        private ProgressDialog progressDialog;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UploadsActivity);

            uploadsList = FindViewById<ListView>(Resource.Id.uploads_list);
            uploadsList.Adapter = new AndroidUtils.ExportedListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());
            uploadsList.ItemClick += OnItemTap;

            uploadAllButton = FindViewById<ToggleButton>(Resource.Id.uploads_start);
            uploadAllButton.Click += uploadAllButton_Click;
        }

        private void MultiUploadComplete(bool isFinal)
        {
            RefreshList();
            if(isFinal)
            {
                RunOnUiThread(() =>
                {
                    uploadAllButton.Checked = false;
                    Toast.MakeText(this, "Finished all uploads!", ToastLength.Long).Show();
                });
            }
        }

        private void uploadAllButton_Click(object sender, EventArgs e)
        {
            ServerData.PushAllResults(MultiUploadComplete);
        }

        /// <summary>
        /// Refresh the listview if the data has changed
        /// </summary>
        private void RefreshList()
        {
            uploadsList.Adapter = null;
            uploadsList.Adapter = new AndroidUtils.ExportedListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());
        }

        private void OnUploadComplete(bool success)
        {
            RunOnUiThread(() => progressDialog.Hide());

            string message = (success) ? "Upload successful!" : "Unable to upload your content. Please try again later.";
            RefreshList();

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetMessage(message);
            alert.SetPositiveButton("Continue", (s, a) =>
            {
            });
            alert.Show();
        }

        public void OnItemTap(object sender, AdapterView.ItemClickEventArgs args)
        {
            if(progressDialog == null)
            {
                progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Uploading your content. Please wait.");
                progressDialog.Indeterminate = true;
            }

            AlertDialog alert = new AlertDialog.Builder(this)
            .SetTitle("Scenario Complete!")
            .SetMessage("What would you like to do with the results of this scenario?")
            .SetCancelable(true)
            .SetNegativeButton("Delete", (EventHandler<DialogClickEventArgs>)null)
            .SetPositiveButton("Upload", (s, a) => {
                IResultItem toUpload = AppData.session.resultsToUpload[args.Position];
                progressDialog.Show();
                ServerData.PushResult(toUpload, OnUploadComplete);
            })
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
                    RefreshList();

                    alert.Dismiss();
                });
                confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                confirm.Show();
            };     
        }
    }
}