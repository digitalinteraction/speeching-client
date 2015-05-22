using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingShared;

namespace DroidSpeeching
{
    [Activity(MainLauncher = true, Theme = "@style/Theme.Splash")]
    public class LoginActivity : Activity, IGoogleApiClientConnectionCallbacks,
        IGoogleApiClientOnConnectionFailedListener, IResultCallback, View.IOnClickListener
    {
        private const int RcSignIn = 8675309;
        private IGoogleApiClient apiClient;
        private ConnectionResult connectionResult;
        private bool intentInProgress;
        private TextView loadingText;
        private bool needLogin;
        private bool revokeAccess;
        private SignInButton signInBtn;
        private bool signInClicked;
        private bool signOut;

        public void OnConnected(Bundle connectionHint)
        {
            if (!needLogin) return;

            if (revokeAccess)
            {
                revokeAccess = false;
                signOut = false;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    PlusClass.AccountApi.ClearDefaultAccount(apiClient);
                    PlusClass.AccountApi.RevokeAccessAndDisconnect(apiClient).SetResultCallback(this);
                });

                return;
            }

            if (signOut)
            {
                signOut = false;
                PlusClass.AccountApi.ClearDefaultAccount(apiClient);
                apiClient.Disconnect();
                apiClient.Connect();
                RunOnUiThread(() => signInBtn.Enabled = true);
                return;
            }

            IPerson currentPerson = PlusClass.PeopleApi.GetCurrentPerson(apiClient);

            if (currentPerson == null) return;

            User thisUser = new User
            {
                Name = currentPerson.DisplayName,
                Id = currentPerson.Id,
                Email = PlusClass.AccountApi.GetAccountName(apiClient)
            };

            thisUser.Nickname = (currentPerson.HasNickname) ? currentPerson.Nickname : thisUser.Name;
            thisUser.Avatar = (currentPerson.Image != null) ? currentPerson.Image.Url : null;

            SetupUserAccount(thisUser);
            needLogin = false;
        }

        /// <summary>
        /// Takes the account created from the Google login and sends it to the server for comparison.
        /// The returned User object contains a Key for authentication in future POST requests
        /// </summary>
        /// <param name="thisUser">The user created from 3rd party systems</param>
        private async void SetupUserAccount(User thisUser)
        {
            User serverUser = await ServerData.PostUserAccount(thisUser);

            if (serverUser == null)
            {
                ThrowError("Failed to set up your account with the service. Please try again later.");
                return;
            }

            AppData.AssignCurrentUser(serverUser);
            ReadyMainMenu();
        }

        public void OnConnectionSuspended(int cause)
        {
            apiClient.Connect();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            if (intentInProgress) return;

            connectionResult = result;

            if (signInClicked)
            {
                ResolveSignInError();
            }
        }

        public void OnClick(View view)
        {
            if (view.Id != Resource.Id.splash_signIn || (apiClient.IsConnecting && !apiClient.IsConnected)) return;

            signInClicked = true;
            ResolveSignInError();
        }

        public void OnResult(Java.Lang.Object result)
        {
            apiClient.Disconnect();
            apiClient.Connect();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Splash);

            signInBtn = FindViewById<SignInButton>(Resource.Id.splash_signIn);
            signInBtn.SetOnClickListener(this);
            signInBtn.Enabled = false;

            loadingText = FindViewById<TextView>(Resource.Id.splash_loading);

            apiClient = new GoogleApiClientBuilder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(PlusClass.Api)
                .AddScope(PlusClass.ScopePlusLogin).Build();

            signOut = Intent.GetBooleanExtra("signOut", false);
            revokeAccess = Intent.GetBooleanExtra("revokeGoogle", false);

            ThreadPool.QueueUserWorkItem(o => AttemptLoad());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode != RcSignIn) return;

            apiClient.Disconnect();

            if (resultCode != Result.Ok)
            {
                signInClicked = false;
            }

            intentInProgress = false;

            if (!apiClient.IsConnecting)
            {
                apiClient.Connect();
            }

            RunOnUiThread(() => signInBtn.Enabled = false);
        }

        private void ResolveSignInError()
        {
            if (connectionResult == null || !connectionResult.HasResolution) return;

            try
            {
                intentInProgress = true;
                connectionResult.StartResolutionForResult(this, RcSignIn);
            }
            catch (Exception)
            {
                // The intent was canceled before it was sent.  Return to the default
                // state and attempt to connect to get an updated ConnectionResult.
                intentInProgress = false;
                apiClient.Connect();
                RunOnUiThread(() => signInBtn.Enabled = true);
            }
        }

        private async void AttemptLoad()
        {
            bool successfulLoad = await AndroidUtils.InitSession(this);

            if (successfulLoad && !signOut)
            {
                RunOnUiThread(() => signInBtn.Enabled = false);

                if (AppData.Session == null || AppData.Session.categories == null ||
                    AppData.Session.categories.Count == 0)
                {
                    // Loaded the user fine, but we need to pull from the server
                    ReadyMainMenu();
                }
                else
                {
                    string name = AppData.Session.currentUser.Nickname;

                    RunOnUiThread(() => Toast.MakeText(this, "Welcome back, " + name + "!", ToastLength.Long).Show());

                    StartActivity(typeof (MainActivity));
                    Finish();
                }
            }
            else
            {
                needLogin = true;
                apiClient.Connect();

                // Unable to load previous session! Allow the user to log in
                RunOnUiThread(() =>
                {
                    signInBtn.Enabled = true;
                    signInBtn.Visibility = ViewStates.Visible;
                    loadingText.Visibility = ViewStates.Gone;
                });
            }
        }

        public async void ReadyMainMenu()
        {
            ProgressDialog dialog = null;

            RunOnUiThread(() =>
            {
                dialog = new ProgressDialog(this);
                dialog.SetTitle("Welcome, " + AppData.Session.currentUser.Nickname + "!");
                dialog.SetMessage("Downloading data. Please wait...");
                dialog.SetCancelable(false);
                dialog.Show();
            });

            bool success = await ServerData.FetchCategories();

            RunOnUiThread(() => { dialog.Hide(); });
            signInClicked = false;

            if (!success)
            {
                ThrowError("Failed to download all of the necessary files. Please check your Internet connection and try again later!");
                return;
            }

            StartActivity(typeof (MainActivity));
            Finish();
        }

        private void ThrowError(string message)
        {
            needLogin = true;

            RunOnUiThread(() =>
            {
                signInBtn.Enabled = true;

                Android.Support.V7.App.AlertDialog errorDialog =
                    new Android.Support.V7.App.AlertDialog.Builder(this)
                        .SetTitle("Network error")
                        .SetMessage(message)
                        .SetPositiveButton("Got it", (par1, par2) => { })
                        .Create();
                errorDialog.Show();
            });

            AppData.Session = null;
        }
    }
}