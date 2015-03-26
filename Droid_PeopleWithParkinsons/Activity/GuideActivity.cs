using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SpeechingCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Droid_PeopleWithParkinsons
{
    /// <summary>
    /// Acts as a slideshow, creating fragments for each page/slide of information which can be swiped between
    /// </summary>
    [Activity(Label = "Guide Activity")]
    public class GuideActivity : FragmentActivity
    {
        GuideAdapter adapter;
        ViewPager pager;
        Dictionary<string, string> resources;
        Guide guide;

        ProgressDialog progress;
        string localZipPath;
        string localResourcesDirectory;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitialiseData();
        }

        /// <summary>
        /// Retrieve the data needed to display the guide. 
        /// May connect to the server or need to download files, so has to be asynchronous
        /// </summary>
        private async Task InitialiseData()
        {
            guide = (Guide)await AppData.session.FetchActivityWithId(Intent.GetIntExtra("ActivityId", 0));
            string scenarioFormatted = guide.Title.Replace(" ", String.Empty).Replace("/", String.Empty);

            string documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/speeching";
            localResourcesDirectory = documentsPath + "/" + scenarioFormatted;

            // Create these directories if they don't already exist
            if (!Directory.Exists(documentsPath))
            {
                Directory.CreateDirectory(documentsPath);
            }

            // If the scenario folder doesn't exist we need to download the additional files
            if (!Directory.Exists(localResourcesDirectory))
            {
                Directory.CreateDirectory(localResourcesDirectory);

                localZipPath = System.IO.Path.Combine(localResourcesDirectory, scenarioFormatted + ".zip");
                PrepareData();
            }
            else
            {
                // We need to populate the resources dictionary with the existing files
                string[] files = Directory.GetFiles(localResourcesDirectory);
                resources = new Dictionary<string, string>();

                for (int i = 0; i < files.Length; i++)
                {
                    resources.Add(System.IO.Path.GetFileName(files[i]), files[i]);
                }
                DisplayContent();
            }
        }

        /// <summary>
        /// Show the content once it is ready
        /// </summary>
        private void DisplayContent()
        {
            SetContentView(Resource.Layout.GuideActivity);
            ActionBar.Hide();

            adapter = new GuideAdapter(SupportFragmentManager, guide.Guides, resources);
            pager = FindViewById<ViewPager>(Resource.Id.guide_pager);
            pager.Adapter = adapter;
            pager.SetPageTransformer(true, new DepthPageTransformer());
        }

        /// <summary>
        /// Downloads the data from the scenario's address
        /// </summary>
        private async void PrepareData()
        {
            RunOnUiThread(() => progress = ProgressDialog.Show(this, "Please Wait", "Downloading data to " + localZipPath, true));
            resources = new Dictionary<string, string>();

            WebClient request = new WebClient();
            await request.DownloadFileTaskAsync(
                new Uri(guide.Resource),
                localZipPath
                );
            request.Dispose();
            request = null;

            RunOnUiThread(() => progress.SetMessage("Unpacking data at " + localZipPath));

            ZipFile zip = null;
            try
            {
                //Unzip the downloaded file and add references to its contents in the resources dictionary
                zip = new ZipFile(File.OpenRead(localZipPath));

                foreach (ZipEntry entry in zip)
                {
                    string filename = System.IO.Path.Combine(localResourcesDirectory, entry.Name);
                    byte[] buffer = new byte[4096];
                    System.IO.Stream zipStream = zip.GetInputStream(entry);
                    using (FileStream streamWriter = File.Create(filename))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    resources.Add(entry.Name, filename);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error! " + e.Message);
            }
            finally
            {
                if (zip != null)
                {
                    zip.IsStreamOwner = true;
                    zip.Close();
                }
            }
            RunOnUiThread(() => progress.Hide());
            DisplayContent();
        }
    }

        public class GuideAdapter : FragmentStatePagerAdapter
        {
            private Android.Support.V4.App.FragmentManager SupportFragmentManager;
            private Guide.Page[] slides;
            private Dictionary<string, string> resources;

            public GuideAdapter(Android.Support.V4.App.FragmentManager SupportFragmentManager, Guide.Page[] slides, Dictionary<string, string> resources)
                : base(SupportFragmentManager)
            {
                this.SupportFragmentManager = SupportFragmentManager;
                this.slides = slides;
                this.resources = resources;
            }

            protected internal readonly string[] _titles;

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                Android.Support.V4.App.Fragment fragment = new GuideFragment();
                Bundle args = new Bundle();
                args.PutString("content", slides[position].Text);
                args.PutString("image", resources[slides[position].MediaLocation]);
                args.PutBoolean("first", position == 0);
                args.PutBoolean("last", position == slides.Length - 1);
                fragment.Arguments = args;
                return fragment;
            }

            public override int Count
            {
                get { return slides.Length; }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(_titles[position]);
            }
        }

        public class DepthPageTransformer : Java.Lang.Object, ViewPager.IPageTransformer
        {
            private static float MIN_SCALE = 0.75f;

            public void TransformPage(View view, float position)
            {
                int pageWidth = view.Width;

                if(position < -1 )
                {
                    // page is off to the left
                    view.Alpha = 0;
                }
                else if(position <= 0)
                {
                    view.Alpha = 1;
                    view.TranslationX = 0;
                    view.ScaleX = 1;
                    view.ScaleY = 1;
                }
                else if(position <= 1)
                {
                    view.Alpha = 1 - position;
                    view.TranslationX = pageWidth * -position;

                    float scaleFactor = MIN_SCALE + (1 - MIN_SCALE) * (1 - Math.Abs(position));
                    view.ScaleX = scaleFactor;
                    view.ScaleY = scaleFactor;
                }
                else
                {
                    view.Alpha = 0;
                }
            }
        }
    }
