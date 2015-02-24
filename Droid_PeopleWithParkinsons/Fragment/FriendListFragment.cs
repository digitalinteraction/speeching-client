using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System;

namespace Droid_PeopleWithParkinsons
{
    public class FriendListFragment : Android.Support.V4.App.Fragment
    {
        private ListView mainList;
        private Button addFriendBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.MainFriendsListFragment, container, false);

            View header = Activity.LayoutInflater.Inflate(Resource.Layout.MainFriendsListHeader, null);
            mainList = view.FindViewById<ListView>(Resource.Id.mainFriendsList);
            mainList.AddHeaderView(header, null, false);
            mainList.Adapter = new UserListAdapter(Activity, Resource.Id.mainFriendsList, AppData.session.friends.ToArray());
            mainList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                this.Activity.StartActivity(typeof(RecordSoundRunActivity));
            };

            addFriendBtn = view.FindViewById<Button>(Resource.Id.addFriendButton);
            addFriendBtn.Click += addFriendBtn_Click;

            return view;
        }

        void addFriendBtn_Click(object sender, System.EventArgs e)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
            alert.SetTitle("Add Friend");
            alert.SetMessage("Enter your friend's username to send a friend request:");

            EditText textInput = new EditText(Activity);
            alert.SetView(textInput);

            alert.SetPositiveButton("Send", (senderAlert, confArgs) =>
            {
                //TODO make awaitable
                bool recognised = AppData.PushFriendRequest(textInput.Text);

                if(recognised)
                {
                    // Redraw the list
                    mainList.Adapter = null;
                    mainList.Adapter = new UserListAdapter(Activity, Resource.Id.mainFriendsList, AppData.session.friends.ToArray());
                }
                else
                {
                    // TODO alert, re-enter text
                }
                
            });
            alert.SetNegativeButton("Cancel", (senderAlert, confArgs) => { });
            alert.SetCancelable(true);
            alert.Show();
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
                if(users[position].status == User.FriendStatus.Denied) return null;

                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Resource.Layout.MainFriendListItem, null);
                }

                view.FindViewById<TextView>(Resource.Id.mainFriendListName).Text = users[position].name;
                view.FindViewById<TextView>(Resource.Id.mainFriendListStatus).Text = users[position].status.ToString();

                return view;
            }
        }
    }
}