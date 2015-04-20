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
    public class AssessmentImgDescFragment : AssessmentTask
    {
        private bool finished = false;
        private string title = "Image Description";
        private string desc = "Describe the image as clearly as you can.";
        private string[] instructions;

        private string imageLoc;
        private ImageView imageView;
        private TextView instructionView;
        private int instructionIndex = 0;

        public AssessmentImgDescFragment(string imageLoc, string[] instructions)
        {
            this.imageLoc = imageLoc;
            this.instructions = instructions;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.ImageDescFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            imageView = view.FindViewById<ImageView>(Resource.Id.describe_image);
            imageView.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(imageLoc)));

            instructionView = view.FindViewById<TextView>(Resource.Id.describe_text);

            if((instructions == null || instructions.Length == 0) && !finished)
            {
                instructionView.Text = "Please describe the image.";
            }
            else
            {
                instructionView.Text = instructions[instructionIndex];
                if (instructionIndex + 1 == instructions.Length) finished = true;
            }
        }

        public override bool IsFinished()
        {
            return finished;
        }

        public override string GetTitle()
        {
            return title;
        }

        public override string GetInstructions()
        {
            return desc;
        }

        public override void NextAction()
        {
            instructionIndex++;
            if (instructionIndex < instructions.Length)
            {
                instructionView.Text = instructions[instructionIndex];

                if (instructionIndex + 1 == instructions.Length) finished = true;
            }
        }

        public override string GetRecordingId()
        {
            // TODO
            return "8675309_" + instructionIndex;
        }
    }
}