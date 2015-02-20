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
using Droid_PeopleWithParkinsons.Shared;

namespace Droid_PeopleWithParkinsons
{
    /// <summary>
    /// A fragment which lists the currently available tasks in a grid layout
    /// </summary>
    public class TaskListFragment : Android.Support.V4.App.Fragment
    {
        private GridView mainList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.MainTaskListFragment, container, false);

            UserTask[] sampleTasks = new UserTask[12];

            for (int i = 0; i < sampleTasks.Length; i++)
            {
                sampleTasks[i] = new UserTask();
                sampleTasks[i].title = "Task " + i;
            }

            mainList = view.FindViewById<GridView>(Resource.Id.mainActivitiesList);
            mainList.Adapter = new UserTaskListAdapter(Activity, Resource.Id.mainActivitiesList, sampleTasks);
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                this.Activity.StartActivity(typeof(RecordSoundRunActivity));
            };

            return view;
        }

    
        public class UserTaskListAdapter : BaseAdapter<UserTask>
        {
            Activity context;
            UserTask[] tasks;

            /// <summary>
            /// An adapter to be able to display the details on each task in a grid or list
            /// </summary>
            public UserTaskListAdapter(Activity context, int resource, UserTask[] data)
            {
                this.context = context;
                this.tasks = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override UserTask this[int position]
            {
                get { return tasks[position]; }
            }

            public override int Count
            {
                get { return tasks.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainMenuListItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.mainListActivityTitle).Text = tasks[position].title;
                return view;
            }
        }
    }
}