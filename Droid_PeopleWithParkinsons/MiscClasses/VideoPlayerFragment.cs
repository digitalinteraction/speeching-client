using System.Threading.Tasks;
using Android.App;
using Android.Net;
using Android.OS;
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

            descriptionView.Text = description;

            video.Prepared += VideoPrepared;
            video.SetVideoURI(Uri.Parse(videoAdd));
            video.SetZOrderOnTop(true); // Removes dimming

            dialog.SetView(view);

            return dialog.Create();
        }


        private void VideoPrepared(object sender, System.EventArgs e)
        {
            prepped = true;
        } 

        public async void StartVideo()
        {
            while (!prepped)
            {
                await Task.Delay(100);
            }
            video.Start();
        }

        public void StopVideo()
        {
            video.StopPlayback();
        }

        public override void OnDestroy()
        {
            if (video.IsPlaying) video.StopPlayback();
            video.Dispose();

            base.OnDestroy();
        }
    
}
}