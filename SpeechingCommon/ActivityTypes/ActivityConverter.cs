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
using Newtonsoft.Json.Linq;

namespace SpeechingCommon
{
    class ActivityConverter : JsonCreationConverter<ISpeechingActivityItem>
    {
        protected override ISpeechingActivityItem Create(Type objectType, JObject jObject)
        {
            if (FieldExists("tasks", jObject))
            {
                return new Scenario();
            }
            else if (FieldExists("slides", jObject))
            {
                return new Guide();
            }
            else
            {
                return new Scenario();
            }
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }
}