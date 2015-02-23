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
            uploadsList.Adapter = new UploadListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());
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
                    uploadsList.Adapter = new UploadListAdapter(this, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());

                    alert.Dismiss();
                });
                confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                confirm.Show();
            };     
        }

        public class UploadListAdapter : BaseAdapter<ResultItem>
        {
            Activity context;
            ResultItem[] results;

            /// <summary>
            /// Display details about a result in a list entry
            /// </summary>
            public UploadListAdapter(Activity context, int resource, ResultItem[] data)
            {
                this.context = context;
                this.results = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ResultItem this[int position]
            {
                get { return results[position]; }
            }

            public override int Count
            {
                get { return results.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.UploadsListItem, null);
                }

                Scenario thisScenario = Scenario.GetWithId(AppData.session.scenarios, results[position].scenarioId);

                view.FindViewById<TextView>(Resource.Id.uploadsList_scenarioTitle).Text = thisScenario.title;
                view.FindViewById<TextView>(Resource.Id.uploadsList_completedAt).Text = "Completed on: " + results[position].completedAt.ToString();

                if(results[position].uploaded)
                {
                    view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Complete!";
                }
                else
                {
                    view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Ready to upload";
                }

                return view;
            }
        }
    }
}