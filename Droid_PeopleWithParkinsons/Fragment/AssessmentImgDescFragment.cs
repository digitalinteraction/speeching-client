using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingShared;
using System;

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
            args.PutString("PROMPTS", JsonConvert.SerializeObject(task.PromptCol));
            args.PutString("IMAGE", task.Image);
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            data = new ImageDescTask
            {
                Id = Arguments.GetInt("ID"),
                Title = Arguments.GetString("TITLE"),
                Instructions = Arguments.GetString("INSTRUCTIONS"),
                PromptCol = JsonConvert.DeserializeObject<AssessmentRecordingPromptCol>(Arguments.GetString("PROMPTS")),
                Image = Arguments.GetString("IMAGE")
            };
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

            if (data.PromptCol == null || data.PromptCol.Prompts.Length == 0)
            {
                instructionView.Text = "Please describe the image.";
            }
            else
            {
                instructionView.Text = data.PromptCol.Prompts[instructionIndex].Value;
                if (instructionIndex + 1 == data.PromptCol.Prompts.Length) finished = true;
            }

            while (runOnceCreated.Count > 0)
            {
                runOnceCreated.Pop().Invoke();
            }

            finishedCreating = true;

            try
            {
                imageView.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(data.Image)));
            }
            catch(Exception e)
            {
                AppData.Io.PrintToConsole(e.Message);
                (Activity as AssessmentActivity).SelfDestruct();
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
            if (instructionIndex < data.PromptCol.Prompts.Length)
            {
                instructionView.Text = data.PromptCol.Prompts[instructionIndex].Value;

                if (instructionIndex + 1 == data.PromptCol.Prompts.Length) finished = true;
            }
        }

        public override int GetRecordingId()
        {
            return data.PromptCol.Prompts[instructionIndex].Id;
        }

        public override string GetRecordingPath()
        {
            return "imgDesc_" + data.Id + "-" + data.PromptCol.Prompts[instructionIndex].Id;
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

        public override IAssessmentTask GetTask()
        {
            return data;
        }
    }
}