using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace DroidSpeeching
{
    public class QuickFireFragment : AssessmentFragment
    {
        private QuickFireTask data;
        private TextView quickFireText;
        private int index = 0;
        private bool finished = false;

        public static QuickFireFragment NewInstance(IAssessmentTask passed)
        {
            QuickFireFragment fragment = new QuickFireFragment();
            QuickFireTask task = passed as QuickFireTask;

            Bundle args = new Bundle();
            args.PutInt("ID", task.Id);
            args.PutString("TITLE", task.Title);
            args.PutString("INSTRUCTIONS", task.Instructions);
            args.PutStringArray("PROMPTS", task.Prompts);
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            data = new QuickFireTask();
            data.Id = Arguments.GetInt("ID");
            data.Title = Arguments.GetString("TITLE");
            data.Instructions = Arguments.GetString("INSTRUCTIONS");
            data.Prompts = Arguments.GetStringArray("PROMPTS");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.QuickfireFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle bundle)
        {
            quickFireText = view.FindViewById<TextView>(Resource.Id.quickfire_text);

            quickFireText.Text = "\"" + data.Prompts[index] + "\"";
            if (index + 1 == data.Prompts.Length) finished = true;

            while (runOnceCreated.Count > 0)
            {
                runOnceCreated.Pop().Invoke();
            }

            finishedCreating = true;

            base.OnViewCreated(view, bundle);
        }

        public override void NextAction()
        {
            index++;
            if (index < data.Prompts.Length)
            {
                quickFireText.Text = "\"" + data.Prompts[index] + "\"";
                
                if (index + 1 == data.Prompts.Length) finished = true;
            }
        }

        public override bool IsFinished()
        {
            return finished;
        }

        public override string GetInstructions()
        {
            return data.Instructions;
        }

        public override string GetTitle()
        {
            return data.Title;
        }

        public override string GetRecordingId()
        {
            return data.Id.ToString() + index;
        }

        public override int GetCurrentStage()
        {
            return index;
        }

        public override void GoToStage(int stage)
        {
            index = stage - 1;
            NextAction();
        }
    }
}