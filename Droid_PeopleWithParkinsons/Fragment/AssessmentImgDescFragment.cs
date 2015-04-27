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

        public static ImageDescFragment NewInstance(IAssessmentTask passed)
        {
            ImageDescFragment fragment = new ImageDescFragment();
            ImageDescTask task = passed as ImageDescTask;

            Bundle args = new Bundle();
            args.PutInt("ID", task.Id);
            args.PutString("TITLE", task.Title);
            args.PutString("INSTRUCTIONS", task.Instructions);
            args.PutStringArray("PROMPTS", task.Prompts);
            args.PutString("IMAGE", task.Image);
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            data = new ImageDescTask();
            data.Id = Arguments.GetInt("ID");
            data.Title = Arguments.GetString("TITLE");
            data.Instructions = Arguments.GetString("INSTRUCTIONS");
            data.Prompts = Arguments.GetStringArray("PROMPTS");
            data.Image = Arguments.GetString("IMAGE");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.ImageDescFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle bundle)
        {
            base.OnViewCreated(view, bundle);

            imageView = view.FindViewById<ImageView>(Resource.Id.describe_image);
            instructionView = view.FindViewById<TextView>(Resource.Id.describe_text);

            if (data.Prompts == null || data.Prompts.Length == 0)
            {
                instructionView.Text = "Please describe the image.";
            }
            else
            {
                instructionView.Text = data.Prompts[instructionIndex];
                if (instructionIndex + 1 == data.Prompts.Length) finished = true;
            }

            while (runOnceCreated.Count > 0)
            {
                runOnceCreated.Pop().Invoke();
            }

            finishedCreating = true;

            imageView.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(data.Image)));
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

        public override int GetCurrentStage()
        {
            return instructionIndex;
        }

        public override void GoToStage(int stage)
        {
            instructionIndex = stage - 1;
            NextAction();
        }
    }
}