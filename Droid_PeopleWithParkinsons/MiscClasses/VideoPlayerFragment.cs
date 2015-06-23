using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DroidSpeeching
{
    public class VideoPlayerFragment : DialogFragment
    {
        private readonly string videoAdd;
        private readonly string title;
        private readonly string description;
        private VideoView video;
        private TextView descriptionView;
        private bool prepped;

        public VideoPlayerFragment(string vidSource, string title, string description)
        {
            videoAdd = vidSource;
            this.title = title;
            this.description = description;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {

            AlertDialog.Builder dialog = new AlertDialog.Builder(Activity)
                .SetTitle(title)
                .SetPositiveButton("Got it", (sender, args) => { });

            LayoutInflater inflater = Activity.LayoutInflater;
            View view = inflater.Inflate(Resource.Layout.VideoHelpPopup, null);

            video = view.FindViewById<VideoView>(Resource.Id.helper_video);
            descriptionView = view.FindViewById<TextView>(Resource.Id.helper_explanation);
            descriptionView.SetText(description.ToCharArray(),0, description.Length);

            if (!string.IsNullOrEmpty(videoAdd))
            {
                video.Prepared += VideoPrepared;
                video.SetVideoURI(Uri.Parse(videoAdd));
                video.Touch += VideoTouched; 
                video.SetZOrderOnTop(true); // Removes dimming
            }
            else
            {
                LinearLayout holder = view.FindViewById<LinearLayout>(Resource.Id.helper_videoHolder);
                holder.Visibility = ViewStates.Gone;
            }

            dialog.SetView(view);

            return dialog.Create();
        }

        private void VideoTouched(object sender, System.EventArgs e)
        {
            if (video != null)
            {
                video.StopPlayback();
                video.Dispose();
                video = null;
            }

            Uri videoUri = Uri.Parse(videoAdd);
            Intent intent = new Intent(Intent.ActionView, videoUri);
            intent.SetDataAndType(videoUri, "video/mp4");
            Activity.StartActivity(intent);
            this.Dismiss();
        } 

        private void VideoPrepared(object sender, System.EventArgs e)
        {
            prepped = true;
        } 

        public async void StartVideo()
        {
            if (string.IsNullOrEmpty(videoAdd)) return;

            while (!prepped)
            {
                await Task.Delay(100);
            }
            video.RequestFocus();
            video.Start();
        }

        public void StopVideo()
        {
            video.StopPlayback();
        }

        public override void OnDestroy()
        {
            if (video != null)
            {
                if (video.IsPlaying) video.StopPlayback();
                video.Dispose();
            }

            base.OnDestroy();
        }
    
}
}