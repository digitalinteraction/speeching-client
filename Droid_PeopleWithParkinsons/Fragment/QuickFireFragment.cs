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
    public class QuickFireFragment : Fragment
    {
        public bool finished = false;

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

            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Toast.MakeText(Activity, "Destroy!", ToastLength.Short).Show();
        }

        public void ShowNextWord()
        {
            if(index < words.Length)
            {
                quickFireText.Text = words[index];
                index++;

                if (index == words.Length) finished = true;
            }
        }

    }
}