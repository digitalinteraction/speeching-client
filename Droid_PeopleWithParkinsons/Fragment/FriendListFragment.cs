using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;

namespace Droid_PeopleWithParkinsons
{
    public class FriendListFragment : Android.Support.V4.App.Fragment
    {
        private ListView mainList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.MainFriendsListFragment, container, false);

            User[] users = new User[12];

            for (int i = 0; i < users.Length; i++)
            {
                users[i] = new User();
                users[i].name = "user " + i;
            }

            mainList = view.FindViewById<ListView>(Resource.Id.mainFriendsList);
            mainList.Adapter = new UserListAdapter(Activity, Resource.Id.mainFriendsList, users);
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                this.Activity.StartActivity(typeof(RecordSoundRunActivity));
            };

            return view;
        }

    
        public class UserListAdapter : BaseAdapter<User>
        {
            Activity context;
            User[] users;

            /// <summary>
            /// An adapter to be able to display the details on each task in a grid or list
            /// </summary>
            public UserListAdapter(Activity context, int resource, User[] data)
            {
                this.context = context;
                this.users = data;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override User this[int position]
            {
                get { return users[position]; }
            }

            public override int Count
            {
                get { return users.Length; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainFriendListItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.mainFriendListName).Text = users[position].name;
                return view;
            }
        }
    }
}