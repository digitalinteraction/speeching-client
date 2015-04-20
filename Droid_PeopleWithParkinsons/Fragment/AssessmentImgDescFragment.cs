using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace DroidSpeeching
{
    public class ImageDescFragment : AssessmentFragment
    {
        private ImageDescTask data;
        private bool finished = false;
        private ImageView imageView;
        private TextView instructionView;
        private int instructionIndex = 0;

        public ImageDescFragment(ImageDescTask data)
        {
            this.data = data;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.ImageDescFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            imageView = view.FindViewById<ImageView>(Resource.Id.describe_image);
            imageView.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(data.Image)));

            instructionView = view.FindViewById<TextView>(Resource.Id.describe_text);

            if((data.Prompts == null || data.Prompts.Length == 0) && !finished)
            {
                instructionView.Text = "Please describe the image.";
            }
            else
            {
                instructionView.Text = data.Prompts[instructionIndex];
                if (instructionIndex + 1 == data.Prompts.Length) finished = true;
            }
        }

        public override bool IsFinished()
        {
            return finished;
        }

        public override string GetTitle()
        {
            return data.Title;
        }

        public override string GetInstructions()
        {
            return data.Instructions;
        }

        public override void NextAction()
        {
            instructionIndex++;
            if (instructionIndex < data.Prompts.Length)
            {
                instructionView.Text = data.Prompts[instructionIndex];

                if (instructionIndex + 1 == data.Prompts.Length) finished = true;
            }
        }

        public override string GetRecordingId()
        {
            // TODO
            return data.Id.ToString() + instructionIndex;
        }
    }
}