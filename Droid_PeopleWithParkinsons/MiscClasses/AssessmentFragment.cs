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

        protected AssessmentFragment()
        {
            runOnceCreated = new Stack<Action>();
        }

        public abstract int GetRecordingId();
        public abstract string GetRecordingPath();
        public abstract bool IsFinished();
        public abstract void NextAction();
        public abstract int GetCurrentStage();
        public abstract void GoToStage(int stage);
        public abstract string GetTitle();
        public abstract ActivityHelp GetHelp();
        public abstract IAssessmentTask GetTask();
    }
}