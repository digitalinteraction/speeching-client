using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SpeechingShared;
using System;
using System.Threading;

namespace Droid_Dysfluency
{
    [Activity(Label = "Uploads", ParentActivity = typeof(MainActivity))]
    public class UploadsActivity : ActionBarActivity
    {
        private ToggleButton uploadAllButton;
        private ListView uploadsList;
        private ProgressDialog progressDialog;
        private CancellationTokenSource cancelTokenSource;

        private bool uploadingAll = false;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UploadsActivity);

            View header = LayoutInflater.Inflate(Resource.Layout.UploadsListHead, null);
            uploadsList = FindViewById<ListView>(Resource.Id.uploads_list);
            uploadsList.AddHeaderView(header, null, false);
            uploadsList.Adapter = new ExportedListAdapter(this, Resource.Id.uploads_list, AppData.Session.ResultsToUpload.ToArray());
            uploadsList.ItemClick += OnItemTap;

            uploadAllButton = FindViewById<ToggleButton>(Resource.Id.uploads_start);
            uploadAllButton.Click += uploadAllButton_Click;
        }

        private void MultiUploadComplete(bool isFinal)
        {
            uploadingAll = false;
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
            if(!uploadingAll)
            {
                uploadingAll = true;
                cancelTokenSource = new CancellationTokenSource();
                ServerData.PushAllResults(RefreshList, MultiUploadComplete, cancelTokenSource.Token);
            }
            else if(cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Refresh the listview if the data has changed
        /// </summary>
        private void RefreshList()
        {
            this.RunOnUiThread(() => {

                uploadsList.Adapter = null;
                uploadsList.Adapter = new ExportedListAdapter(this, Resource.Id.uploads_list, AppData.Session.ResultsToUpload.ToArray());
            
            });
           
        }

        private void OnUploadComplete(bool success)
        {
            RunOnUiThread(() => progressDialog.Hide());

            string message = (success) ? "Upload successful!" : "Unable to upload your content. Please try again later.";
            RefreshList();

            Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
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
                progressDialog.SetButton("Cancel Upload", (arg1, arg2) => { if (cancelTokenSource != null) cancelTokenSource.Cancel(); });
                progressDialog.Indeterminate = true;
            }

            Android.Support.V7.App.AlertDialog alert = new Android.Support.V7.App.AlertDialog.Builder(this)
            .SetTitle("Scenario Complete!")
            .SetMessage("What would you like to do with the results of this scenario?")
            .SetCancelable(true)
            .SetNegativeButton("Delete", (EventHandler<DialogClickEventArgs>)null)
            .SetPositiveButton("Upload", (s, a) => {
                IResultItem toUpload = AppData.Session.ResultsToUpload[args.Position - 1];
                progressDialog.Show();
                cancelTokenSource = new CancellationTokenSource();
                ServerData.PushResult(toUpload, RefreshList, OnUploadComplete, cancelTokenSource.Token);
            })
            .SetNeutralButton("Cancel", (s, a) => { })
            .Create();

            alert.Show();

            // A second alert dialogue, confirming the decision to delete
            Button negative = alert.GetButton((int)DialogButtonType.Negative);
            negative.Click += delegate(object s, EventArgs e)
            {
                Android.Support.V7.App.AlertDialog.Builder confirm = new Android.Support.V7.App.AlertDialog.Builder(this);
                confirm.SetTitle("Are you sure?");
                confirm.SetMessage("The recorded data will be irrecoverably lost.");
                confirm.SetPositiveButton("Delete", (senderAlert, confArgs) =>
                {
                    AppData.Session.DeleteResult(AppData.Session.ResultsToUpload[args.Position - 1]);
                    RefreshList();

                    alert.Dismiss();
                });
                confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                confirm.Show();
            };     
        }
    }  
}