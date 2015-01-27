using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace Droid_PeopleWithParkinsons
{
    static class ModelManager
    {
        private static string fileName = "model.txt";
        private static string savePath
        {
            get
            {
                string dir = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "/model/");

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "/model/");
            }
        }

        private static bool initialised = false;

        private static List<SentenceModel> _uploads;
        public static IReadOnlyList<SentenceModel> uploads
        {
            get
            {
                if (!initialised)
                {
                    Initialise();
                }

                return _uploads.AsReadOnly();
            }
        }

        private static void Initialise()
        {
            ReadFromFile();
            initialised = true;
        }

        private static void SaveToFile()
        {
            if (!initialised)
            {
                Initialise();
            }

            string json = JsonConvert.SerializeObject(_uploads);

            using (var file = File.Open(savePath + fileName, FileMode.Create, FileAccess.Write))
            using (var strm = new StreamWriter(file))
            {
                strm.Write(json);
            }
        }

        private static void ReadFromFile()
        {
            if (File.Exists(savePath + fileName))
            {
                _uploads = JsonConvert.DeserializeObject<List<SentenceModel>>(File.ReadAllText(savePath + fileName));
            }
            else
            {
                _uploads = new List<SentenceModel>();
            }
        }

        public static void AddModel(SentenceModel model)
        {
            if (!initialised)
            {
                Initialise();
            }

            _uploads.Add(model);
            SaveToFile();
        }

        public static void DeleteModel(SentenceModel model)
        {
            if (!initialised)
            {
                Initialise();
            }

            _uploads.Remove(model);
            SaveToFile();
        }
    }
}
