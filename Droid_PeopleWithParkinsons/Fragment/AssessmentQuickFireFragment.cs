using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DroidSpeeching
{
    public class QuickFireFragment : AssessmentTask
    {
        private bool finished = false;

        private string title = "Quickfire Speaking";
        private string desc = "Press the record button and say the shown word as clearly as you can, then press stop.";
        private int index = 0;
        private string[] words;
        private TextView quickFireText;

        public QuickFireFragment(string[] toShow)
        {
            words = toShow;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.QuickfireFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            quickFireText = view.FindViewById<TextView>(Resource.Id.quickfire_text);
            quickFireText.Text = words[index];
            if (index + 1 == words.Length) finished = true;

            base.OnViewCreated(view, savedInstanceState);
        }

        public override void NextAction()
        {
            index++;
            if (index < words.Length)
            {
                quickFireText.Text = words[index];
                
                if (index + 1 == words.Length) finished = true;
            }
        }

        public override bool IsFinished()
        {
            return finished;
        }

        public override string GetInstructions()
        {
            return desc;
        }

        public override string GetTitle()
        {
            return title;
        }

        public override string GetRecordingId()
        {
            // TODO
            return "19920407_" + index;
        }
    }
}