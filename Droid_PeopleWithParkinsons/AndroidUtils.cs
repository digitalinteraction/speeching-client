using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Droid_PeopleWithParkinsons
{
    class AndroidUtils
    {
        public static void AddTab(string tabName, Bundle currentBundle, Activity activity, int fragContainerId, Fragment view )
        {
            // Do actionbar tab setup - Each tab is a fragment
            ActionBar.Tab tab = activity.ActionBar.NewTab();
            tab.SetText(tabName);
            tab.TabSelected += (sender, args) =>
            {
                if (currentBundle == null)
                {
                    currentBundle = activity.Intent.Extras == null ? new Bundle() : activity.Intent.Extras;
                }

                var fragment = activity.FragmentManager.FindFragmentById(fragContainerId);
                
                if(fragment != null)
                {
                    args.FragmentTransaction.Remove(fragment);
                }
                args.FragmentTransaction.Add(fragContainerId, view);
            };

            activity.ActionBar.AddTab(tab);
        }

    }
}