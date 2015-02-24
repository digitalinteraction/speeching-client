using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System;

namespace Droid_PeopleWithParkinsons
{
    public class SubmittedListFragment : Android.Support.V4.App.Fragment
    {
        private ListView exportList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.SubmittedFragment, container, false);

            View header = Activity.LayoutInflater.Inflate(Resource.Layout.SubmittedHeader, null);

            exportList = view.FindViewById<ListView>(Resource.Id.submitted_list);
            exportList.AddHeaderView(header, null, false);
            exportList.Adapter = new AndroidUtils.ExportedListAdapter(Activity, Resource.Id.submitted_list, AppData.FetchSubmittedResults());
            exportList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                // The list's header borks indexing
                ResultItem res = AppData.session.resultsOnServer[args.Position - 1]; //TEMP

                View alertView = Activity.LayoutInflater.Inflate(Resource.Layout.SubmittedAlert, null);

                Button feedbackBtn = alertView.FindViewById<Button>(Resource.Id.submittedAlert_feedbackBtn);
                feedbackBtn.Click += feedbackBtn_Click;

                Button permissionsBtn = alertView.FindViewById<Button>(Resource.Id.submittedAlert_permission);
                permissionsBtn.Click += permissionsBtn_Click;

                AlertDialog alert = new AlertDialog.Builder(Activity)
                .SetTitle(res.completedAt.ToShortDateString() + " submission for '" + Scenario.GetWithId(AppData.session.scenarios, res.scenarioId).title + "'")
                .SetView(alertView)
                .SetCancelable(true)
                .SetNegativeButton("Delete", (EventHandler<DialogClickEventArgs>)null)
                .SetNeutralButton("Close", (s, a) => { })
                .Create();

                alert.Show();

                // A second alert dialogue, confirming the decision to delete
                Button deleteBtn = alert.GetButton((int)DialogButtonType.Negative);
                deleteBtn.Click += delegate(object s, EventArgs e)
                {
                    AlertDialog.Builder confirm = new AlertDialog.Builder(Activity);
                    confirm.SetTitle("Are you sure?");
                    confirm.SetMessage("The recorded data will be deleted from the server and irrecoverably lost. Continue?");
                    confirm.SetPositiveButton("Delete", (senderAlert, confArgs) =>
                    {
                        AppData.PushResultDeletion(res);

                        exportList.Adapter = null;
                        exportList.Adapter = new AndroidUtils.ExportedListAdapter(Activity, Resource.Id.uploads_list, AppData.FetchSubmittedResults());

                        alert.Dismiss();
                    });
                    confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                    confirm.Show();
                };     
            };

            return view;
        }

        void permissionsBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(Activity, "Permissions", ToastLength.Short).Show();
        }

        void feedbackBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(Activity, "Feedback", ToastLength.Short).Show();
        }
    }
}