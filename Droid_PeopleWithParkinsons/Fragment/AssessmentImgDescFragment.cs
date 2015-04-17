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

        private string imageLoc;
        private ImageView imageView;

        public AssessmentImgDescFragment(string imageLoc)
        {
            this.imageLoc = imageLoc;
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
            finished = true;
        }
    }
}