using Android.OS;
using Android.Views;
using Android.Widget;

namespace DroidSpeeching
{
    /// <summary>
    /// A simple fragment which acts as a page in a guide activity. 
    /// The layout changes depending on the page's position (first, last etc) in the guide.
    /// This and the page's content is determined by the data passed in the passed Bundle
    /// </summary>
    public class GuideFragment : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.GuideFragment, container, false);
            Bundle args = this.Arguments;

            view.FindViewById<TextView>(Resource.Id.guide_content).Text = args.GetString("content");

            ImageView bg = view.FindViewById<ImageView>(Resource.Id.guide_mainImage);
            bg.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(args.GetString("image"))));

            view.FindViewById<LinearLayout>(Resource.Id.guide_left).Visibility = (args.GetBoolean("first")) ? ViewStates.Gone : ViewStates.Visible;
            view.FindViewById<LinearLayout>(Resource.Id.guide_right).Visibility = (args.GetBoolean("last")) ? ViewStates.Gone : ViewStates.Visible;
            return view;
        }
    }
}