using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Gcm;
using Android.Gms.Location;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.OS;
using Android.Views;
using Android.Widget;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DroidSpeeching
{
    [Activity( MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash")]
    public class SplashActivity : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback, Android.Views.View.IOnClickListener
    {
        IGoogleApiClient apiClient;
        bool intentInProgress = false;
        bool signInClicked = false;
        int RC_SIGN_IN = 0;
        ConnectionResult connectionResult;

        TextView loadingText;
        SignInButton signInBtn;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Splash);

            signInBtn = FindViewById<SignInButton>(Resource.Id.splash_signIn);
            signInBtn.SetOnClickListener(this);

            loadingText = FindViewById<TextView>(Resource.Id.splash_loading);

            GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this)
            .AddConnectionCallbacks(this)
            .AddOnConnectionFailedListener(this)
            .AddApi(PlusClass.Api)
            .AddScope(PlusClass.ScopePlusLogin);

            apiClient = builder.Build();

            ThreadPool.QueueUserWorkItem(o => AttemptLoad());
        }

        public void OnClick(View view)
        {
            if(view.Id == Resource.Id.splash_signIn && !apiClient.IsConnecting)
            {
                signInClicked = true;
                apiClient.Connect();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if(requestCode == RC_SIGN_IN)
            {
                if(resultCode != Result.Ok)
                {
                    signInClicked = false;
                }

                intentInProgress = false;

                if(!apiClient.IsConnecting)
                {
                    apiClient.Connect();
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        private void ResolveSignInError()
        {
            if (connectionResult.HasResolution)
            {
                try
                {
                    intentInProgress = true;
                    StartIntentSenderForResult(connectionResult.Resolution.IntentSender,
                        RC_SIGN_IN, null, 0, 0, 0);
                }
                catch (Exception e)
                {
                    // The intent was canceled before it was sent.  Return to the default
                    // state and attempt to connect to get an updated ConnectionResult.
                    intentInProgress = false;
                    apiClient.Connect();
                }
            }
        }

        private async Task AttemptLoad()
        {
            bool successfulLoad = await AndroidUtils.InitSession(this);

            if(successfulLoad)
            {
                if (AppData.session.categories == null || AppData.session.categories.Count == 0)
                {
                    // Loaded the user fine, but we need to pull from the server
                    ReadyMainMenu();
                }
                else
                {
                    StartActivity(typeof(MainActivity));
                }
            }   
            else
            {
                // Unable to load previous session! Allow the user to log in
                RunOnUiThread(() => 
                    {
                        signInBtn.Visibility = ViewStates.Visible;
                        loadingText.Visibility = ViewStates.Gone;
                    });
            }
        }


        public void OnConnected(Bundle connectionHint)
        {
            IPerson currentPerson = PlusClass.PeopleApi.GetCurrentPerson(apiClient);

            if(currentPerson != null)
            {
                User thisUser = new User();
                thisUser.name = currentPerson.DisplayName;
                thisUser.nickname = (currentPerson.HasNickname) ? currentPerson.Nickname : thisUser.name;
                thisUser.id = currentPerson.Id;
                thisUser.avatar = (currentPerson.Image != null) ? currentPerson.Image.Url : null;
                thisUser.email = PlusClass.AccountApi.GetAccountName(apiClient);

                AppData.AssignCurrentUser(thisUser);
                ReadyMainMenu();
            }
        }

        public async Task ReadyMainMenu()
        {
            ProgressDialog dialog = new ProgressDialog(this);
            dialog.SetTitle("Welcome, " + AppData.session.currentUser.nickname + "!");
            dialog.SetMessage("Downloading data. Please wait...");
            dialog.SetCancelable(false);
            dialog.Show();

            bool success = await ServerData.FetchCategories();

            dialog.Hide();

            signInClicked = false;

            if(success)
            {
                StartActivity(typeof(MainActivity));
            }
            else
            {
                AlertDialog alert = new AlertDialog.Builder(this)
                    .SetTitle("Connection error")
                    .SetMessage("We were unable to reach the server - please try again later.")
                    .SetPositiveButton("Ok", (arg1, arg2) => { })
                    .Create();
                alert.Show();
            }
            
        }

        public void OnConnectionSuspended(int cause)
        {
            apiClient.Connect();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            if(!intentInProgress)
            {
                connectionResult = result;

                if(signInClicked)
                {
                    ResolveSignInError();
                }
            }
        }

        public void OnResult(Java.Lang.Object result)
        {
            try
            {
                IPeopleLoadPeopleResult thisRes = result.Cast<IPeopleLoadPeopleResult>();
                string todo;
            }
            catch(Exception ex)
            {
                return;
            }
        }
    }
    
}