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
            exportList.AddHeaderView(header);
            exportList.Adapter = new AndroidUtils.ExportedListAdapter(Activity, Resource.Id.submitted_list, AppData.GetSubmittedResults());
            exportList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                ResultItem res = AppData.session.resultsOnServer[args.Position - 1]; //TEMP

                AlertDialog alert = new AlertDialog.Builder(Activity)
                .SetTitle("Your submission for '" + Scenario.GetWithId(AppData.session.scenarios, res.scenarioId).title + "'")
                .SetMessage("What would you like to do with the results of this scenario?")
                .SetCancelable(true)
                .SetNegativeButton("Delete From Server", (EventHandler<DialogClickEventArgs>)null)
                .SetNeutralButton("Close", (s, a) => { })
                .Create();

                alert.Show();

                // A second alert dialogue, confirming the decision to delete
                Button negative = alert.GetButton((int)DialogButtonType.Negative);
                negative.Click += delegate(object s, EventArgs e)
                {
                    AlertDialog.Builder confirm = new AlertDialog.Builder(Activity);
                    confirm.SetTitle("Are you sure?");
                    confirm.SetMessage("The recorded data will be irrecoverably lost.");
                    confirm.SetPositiveButton("Delete", (senderAlert, confArgs) =>
                    {
                        AppData.session.DeleteResult(AppData.session.resultsToUpload[args.Position]);

                        exportList.Adapter = null;
                        exportList.Adapter = new AndroidUtils.ExportedListAdapter(Activity, Resource.Id.uploads_list, AppData.session.resultsToUpload.ToArray());

                        alert.Dismiss();
                    });
                    confirm.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
                    confirm.Show();
                };     
            };

            return view;
        }
    }
}