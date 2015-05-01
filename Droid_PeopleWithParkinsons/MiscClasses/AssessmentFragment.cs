using Android.App;
using SpeechingShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DroidSpeeching
{
    public abstract class AssessmentFragment : Fragment
    {
        public Stack<Action> runOnceCreated;
        public bool finishedCreating = false;

        public AssessmentFragment()
        {
            runOnceCreated = new Stack<Action>();
        }

        public abstract string GetRecordingId();
        public abstract bool IsFinished();
        public abstract void NextAction();
        public abstract int GetCurrentStage();
        public abstract void GoToStage(int stage);
        public abstract string GetTitle();
        public abstract string GetInstructions();
    }
}