using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SpeechingShared;

namespace DroidSpeeching
{
    public class QuickFireFragment : AssessmentFragment
    {
        public QuickFireTask Data;
        public int Index = 0;
        private TextView quickFireText;
        private bool finished = false;

        public static QuickFireFragment NewInstance(IAssessmentTask passed)
        {
            QuickFireFragment fragment = new QuickFireFragment();
            QuickFireTask task = passed as QuickFireTask;

            Bundle args = new Bundle();
            args.PutInt("ID", task.Id);
            args.PutString("TITLE", task.Title);
            args.PutString("INSTRUCTIONS", task.Instructions);
            args.PutString("PROMPTS", JsonConvert.SerializeObject(task.PromptCol));
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Data = new QuickFireTask
            {
                Id = Arguments.GetInt("ID"),
                Title = Arguments.GetString("TITLE"),
                Instructions = Arguments.GetString("INSTRUCTIONS"),
                PromptCol = JsonConvert.DeserializeObject<AssessmentRecordingPromptCol>(Arguments.GetString("PROMPTS"))
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.QuickfireFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle bundle)
        {
            quickFireText = view.FindViewById<TextView>(Resource.Id.quickfire_text);

            quickFireText.Text = "\"" + Data.PromptCol.Prompts[Index].Value + "\"";
            if (Index + 1 == Data.PromptCol.Prompts.Length) finished = true;

            while (runOnceCreated.Count > 0)
            {
                runOnceCreated.Pop().Invoke();
            }

            finishedCreating = true;

            base.OnViewCreated(view, bundle);
        }

        public override void NextAction()
        {
            Index++;
            if (Index < Data.PromptCol.Prompts.Length)
            {
                quickFireText.Text = "\"" + Data.PromptCol.Prompts[Index].Value + "\"";
                
                if (Index + 1 == Data.PromptCol.Prompts.Length) finished = true;
            }
        }

        public override bool IsFinished()
        {
            return finished;
        }

        public override string GetInstructions()
        {
            return Data.Instructions;
        }

        public override string GetTitle()
        {
            return Data.Title;
        }

        public override int GetRecordingId()
        {
            return Data.PromptCol.Prompts[Index].Id;
        }

        public override string GetRecordingPath()
        {
            return "quickfire_" + Data.Id + "-" + Data.PromptCol.Prompts[Index].Id;
        }

        public override int GetCurrentStage()
        {
            return Index;
        }

        public override void GoToStage(int stage)
        {
            Index = stage - 1;
            NextAction();
        }

        public override IAssessmentTask GetTask()
        {
            return Data;
        }
    }
}