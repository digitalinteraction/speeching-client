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

        public QuickFireFragment(QuickFireTask data)
        {
            this.data = data;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.QuickfireFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if(data.Prompts != null)
            {
                quickFireText = view.FindViewById<TextView>(Resource.Id.quickfire_text);
                quickFireText.Text = "\"" + data.Prompts[index] + "\"";
                if (index + 1 == data.Prompts.Length) finished = true;
            }
            
            base.OnViewCreated(view, savedInstanceState);
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
    }
}