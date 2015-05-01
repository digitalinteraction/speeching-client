using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    class AssessmentConverter : JsonCreationConverter<IAssessmentTask>
    {
        protected override IAssessmentTask Create(Type objectType, JObject jObject)
        {
            if (FieldExists("Image", jObject))
            {
                return new ImageDescTask();
            }
            else
            {
                return new QuickFireTask();
            }
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }
}