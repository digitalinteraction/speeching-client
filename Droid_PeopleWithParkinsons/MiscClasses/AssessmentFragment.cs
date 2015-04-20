using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DroidSpeeching
{
    public abstract class AssessmentFragment : Fragment
    {
        public abstract string GetRecordingId();
        public abstract bool IsFinished();
        public abstract void NextAction();
        public abstract string GetTitle();
        public abstract string GetInstructions();
    }
}