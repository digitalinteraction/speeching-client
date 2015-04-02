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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Splash);

            FindViewById(Resource.Id.sign_in_button).SetOnClickListener(this);

            GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this)
            .AddConnectionCallbacks(this)
            .AddOnConnectionFailedListener(this)
            .AddApi(PlusClass.Api)
            .AddScope(PlusClass.ScopePlusLogin);

            apiClient = builder.Build();

            //ThreadPool.QueueUserWorkItem(o => CreateData());
        }

        public void OnClick(View view)
        {
            if(view.Id == Resource.Id.sign_in_button && !apiClient.IsConnecting)
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

        private async Task CreateData()
        {
            try
            {
                bool successfulLoad = await AndroidUtils.InitSession(this);

                if(successfulLoad)
                    StartActivity(typeof(MainActivity));
                else
                {
                    RunOnUiThread(() => 
                        {
                            AlertDialog alert = new AlertDialog.Builder(this)
                                .SetTitle("Internet connection required!")
                                .SetMessage("We were unable to load offline data and failed to connect to the service. Please try again later.")
                                .SetCancelable(false)
                                .SetPositiveButton("Ok", (p1, p2) => { Finish(); })
                                .Create();
                            alert.Show();  
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Err: " + e);
            }
        }


        public void OnConnected(Bundle connectionHint)
        {
            //Huzzah!
            signInClicked = false;

            IPerson currentPerson = PlusClass.PeopleApi.GetCurrentPerson(apiClient);

            if(currentPerson != null)
            {
                string name = currentPerson.DisplayName;
                IPersonImage photo = currentPerson.Image;
                string plusProfile = currentPerson.Url;
                string email = PlusClass.AccountApi.GetAccountName(apiClient);

                Toast.MakeText(this, "Yay data", ToastLength.Short).Show();
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