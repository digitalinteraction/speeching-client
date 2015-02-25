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
using SpeechingCommon;
using Android.Util;

namespace Droid_PeopleWithParkinsons
{
    [Activity(Label = "Manage Permissions")]
    public class PermissionsActivity : Activity
    {
        private ResultItem resultItem;
        private ListView allowedList;
        private ListView friendList;
        private TextView headerText;

        private Button addFriendBtn;
        private Button addOtherBtn;
        private Button makePublicBtn;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.PermissionsActivity);

            addFriendBtn = FindViewById<Button>(Resource.Id.permissions_addFriendsButton);
            addFriendBtn.Click += addFriendBtn_Click;

            addOtherBtn = FindViewById<Button>(Resource.Id.permissions_addOtherButton);
            addOtherBtn.Click += addOtherBtn_Click;

            makePublicBtn = FindViewById<Button>(Resource.Id.permissions_publicButton);
            makePublicBtn.Click += makePublicBtn_Click;

            resultItem = AppData.FetchSubmittedResult(Intent.GetStringExtra("ResultId"));
            View header = LayoutInflater.Inflate(Resource.Layout.PermissionsListHeader, null);
            headerText = header.FindViewById<TextView>(Resource.Id.permissions_message);
            allowedList = FindViewById<ListView>(Resource.Id.permissions_list);
            allowedList.AddHeaderView(header, null, false);
            UpdateLayout();
            allowedList.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs args)
            {
                User thisUser = AndroidUtils.Cast<User>(allowedList.Adapter.GetItem(args.Position));

                AndroidUtils.CreateAlert(this,
                "Remove Permissions?","Do you really want to revoke " + thisUser.name + "'s ability to access this uploaded content?",
                "Revoke Permission", (senderAlert, confArgs) =>
                {
                    resultItem.allowedUsers.Remove(thisUser.id);
                    resultItem.PushPermissionUpdates();
                    UpdateLayout();
                },
                "Cancel", (senderAlert, confArgs) => { });
            };
        }

        /// <summary>
        /// Gives the user the option of flipping the content's public accessibility
        /// </summary>
        void makePublicBtn_Click(object sender, EventArgs e)
        {
            bool pub = resultItem.isPublic;

            string status = (pub) ? "private" : "public";
            string effect = (pub) ? "only invited users will be able to access it" : "anyone will be able to access it";

            AndroidUtils.CreateAlert(this,
                "Make this content "+ status +"?",
                "If you make this content " + status + ", "+ effect+". Are you sure you want to do this?",
                "Make " + status, (senderAlert, confArgs) => {
                    resultItem.isPublic = !resultItem.isPublic;
                    resultItem.PushPermissionUpdates();
                    UpdateLayout();
                },
                "Cancel", (senderAlert, confArgs) => { });
        }

        void addOtherBtn_Click(object sender, EventArgs e)
        {
            EditText textInput = new EditText(this);
            AlertDialog alert = new AlertDialog.Builder(this)
            .SetTitle("Add another user")
            .SetMessage("Enter the person's username to give access permissions for this content:")
            .SetView(textInput)
            .SetPositiveButton("Give Permission", (EventHandler<DialogClickEventArgs>)null)
            .SetNegativeButton("Cancel", (senderAlert, confArgs) => { })
            .SetCancelable(true)
            .Create();

            alert.Show();

            Button positive = alert.GetButton((int)DialogButtonType.Positive);
            positive.Click += delegate(object clickSender, EventArgs args)
            {
                //TODO make awaitable
                User foundUser = AppData.FetchUser(textInput.Text);

                if (foundUser != null)
                {
                    resultItem.allowedUsers.Add(foundUser.id);
                    UpdateLayout();
                    alert.Dismiss();
                }
                else
                {
                    AlertDialog.Builder confirm = new AlertDialog.Builder(this);
                    confirm.SetTitle("User not found!");
                    confirm.SetMessage("No user was found with the given username!");
                    confirm.SetPositiveButton("Ok", (senderAlert, confArgs) => { });
                    confirm.Show();
                }
            };   
        }

        /// <summary>
        /// Updates the layout to reflect the latest data
        /// </summary>
        private void UpdateLayout()
        {
            User[] allowedUsers = AppData.FetchUsers(resultItem.allowedUsers);

            if(resultItem.isPublic)
            {
                headerText.Text = "This content is currently publicly available, meaning that anyone can access its contents.";
            }
            else
            {
                headerText.Text = (allowedUsers.Length > 0) ? "Add or remove other users' ability to access this uploaded content." :
                                                         "No one can currently access this content! Give some users permission to start getting feedback.";
            }

            makePublicBtn.Text = (resultItem.isPublic) ? "Make private" : "Make public";

            AppData.SaveCurrentData();

            allowedList.Adapter = null;
            allowedList.Adapter = new FriendListFragment.UserListAdapter(this, Resource.Id.mainFriendsList, AppData.FetchUsers(resultItem.allowedUsers));
        }

        /// <summary>
        /// Show a multiple choice list of users from the friends list who haven't already been given permissions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void addFriendBtn_Click(object sender, EventArgs e)
        {
            List<User> notAdded = new List<User>();
            List<User> allFriends = AppData.FetchAcceptedFriends();

            foreach(User user in allFriends)
            {
                if(!resultItem.allowedUsers.Contains(user.id))
                {
                    notAdded.Add(user);
                }
            }

            // Don't show the alert dialog if there aren't any users to choose from
            if(notAdded.Count == 0)
            {
                string toastText = (allFriends.Count > 0) ? "No more friends available" : "No friends available";
                Toast.MakeText(this, toastText, ToastLength.Long).Show();
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            View alertView = LayoutInflater.Inflate(Resource.Layout.FriendsCheckListAlert, null);
            alert.SetView(alertView);

            friendList = null;
            friendList = alertView.FindViewById<ListView>(Resource.Id.friendsCheckAlert_List);
            friendList.Adapter = new FriendsCheckListAdapter(this, Resource.Id.friendsCheckAlert_List, notAdded.ToArray());
            friendList.ChoiceMode = ChoiceMode.Multiple;

            alert.SetNeutralButton("Cancel", (senderAlert, confArgs) => { });
            alert.SetPositiveButton("Give Access", (senderAlert, confArgs) => {

                // Give all users who were checked permission rights
                SparseBooleanArray checkeditems = friendList.CheckedItemPositions;

                for (int i = 0; i < checkeditems.Size(); i++)
                {
                    int pos = checkeditems.KeyAt(i);

                    if (checkeditems.ValueAt(i))
                    {
                        resultItem.allowedUsers.Add(AndroidUtils.Cast<User>(friendList.Adapter.GetItem(pos)).id);
                    }
                }

                resultItem.PushPermissionUpdates();

                UpdateLayout();
            });

            alert.Show();
        }

        public class FriendsCheckListAdapter : BaseAdapter<User>
        {
            Activity context;
            User[] users;

            public FriendsCheckListAdapter(Activity context, int resource, User[] data)
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
                if (users[position].status == User.FriendStatus.Denied) return null;

                View view = convertView;

                if (view == null)
                {
                    view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemMultipleChoice, null);
                }

                view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = users[position].name;

                return view;
            }
        }

    }
}