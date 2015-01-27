using System;
using System.Collections.Generic;
using System.Text;

using Android.Content;

namespace Droid_PeopleWithParkinsons
{
    static class SentenceManager
    {
        public static int MIN_STORED_SENTENCES = 10;
        public static int DOWNLOAD_SENTENCES = 15;

        private static Random random = new Random();
        private static bool initialised = false;
        private static List<string> _sentences;

        public static IReadOnlyCollection<string> sentences
        {
            get
            {
                if (!initialised)
                {
                   Initialise();
                }

                return _sentences.AsReadOnly();
            }
        }

        private static void Initialise()
        {
             ISharedPreferences prefs = Android.App.Application.Context.ApplicationContext.GetSharedPreferences("com.speeching.speeching.QUESTIONS", FileCreationMode.Private);
             _sentences = new List<string>(prefs.GetStringSet("questions", new List<string>()));
             initialised = true;
        }


        private static void SaveQuestions()
        {
            ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences("MyApp", FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutStringSet("questions", _sentences);
            prefEditor.Commit();
        }


        public static void AddSentence(string question)
        {
            if (!initialised)
            {
                Initialise();
            }

            _sentences.Add(question);
            SaveQuestions();
        }



        public static bool DeleteQuestion(string question)
        {
            if (!initialised)
            {
                Initialise();
            }

            if (_sentences.Contains(question))
            {
                _sentences.Remove(question);
                SaveQuestions();
                return true;
            }

            return false;
        }


        public static string GetRandomQuestion()
        {
            if (!initialised)
            {
                Initialise();
            }

            if (random == null)
            {
                random = new Random();
            }

            if (_sentences.Count > 0)
            {
                return _sentences[random.Next(0, _sentences.Count)];
            }
            else
            {
                return null;
            }
        }
    }
}
