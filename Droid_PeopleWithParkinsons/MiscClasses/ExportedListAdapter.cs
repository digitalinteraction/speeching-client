using Android.App;
using Android.Views;
using Android.Widget;
using SpeechingShared;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    /// <summary>
    /// A list adapter for the results and responses that the user has exported
    /// </summary>
    public class ExportedListAdapter : BaseAdapter<IResultItem>
    {
        Activity context;
        IResultItem[] results;

        /// <summary>
        /// Display details about a result in a list entry
        /// </summary>
        public ExportedListAdapter(Activity context, int resource, IResultItem[] data)
        {
            this.context = context;
            this.results = data;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override IResultItem this[int position]
        {
            get { return results[position]; }
        }

        public override int Count
        {
            get { return results.Length; }
        }

        private async void PopulateActivityResultView(int activityId, View view)
        {
            ISpeechingActivityItem thisItem = await AppData.Session.FetchActivityWithId(activityId);

            view.FindViewById<TextView>(Resource.Id.uploadsList_scenarioTitle).Text = thisItem.Title;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.UploadsListItem, null);
            }

            if (results[position].GetType() == typeof(LocationRecordingResult))
            {
                view.FindViewById<TextView>(Resource.Id.uploadsList_scenarioTitle).Text = "Log about " + ((LocationRecordingResult)results[position]).GooglePlaceName;
            }
            else if(results[position].GetType() == typeof(ScenarioResult) && (results[position] as ScenarioResult).isAssessment)
            {
                view.FindViewById<TextView>(Resource.Id.uploadsList_scenarioTitle).Text = "Assessment Results";
            }
            else
            {
                PopulateActivityResultView(results[position].ParticipantActivityId, view);
            }

            view.FindViewById<TextView>(Resource.Id.uploadsList_completedAt).Text = "Completed on: " + results[position].CompletionDate.ToString();

            bool showProgress = false;

            if (results[position].UploadState == Utils.UploadStage.Uploading)
            {
                view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Uploading...";
                showProgress = true;
            }
            else if (results[position].UploadState == Utils.UploadStage.OnStorage)
            {
                view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Notifying service...";
                showProgress = true;
            }
            else if (results[position].UploadState == Utils.UploadStage.Ready)
            {
                view.FindViewById<TextView>(Resource.Id.uploadsList_uploadStatus).Text = "Ready to upload";
            }

            ProgressBar prog = view.FindViewById<ProgressBar>(Resource.Id.uploadsList_progress);
            ImageView icon = view.FindViewById<ImageView>(Resource.Id.uploadsList_icon);
            if(showProgress)
            {
                icon.Visibility = ViewStates.Gone;
                prog.Visibility = ViewStates.Visible;
                prog.Indeterminate = true;
            }
            else
            {
                icon.Visibility = ViewStates.Visible;
                prog.Visibility = ViewStates.Gone;
            }

            return view;
        }
    }
}