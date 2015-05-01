using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public struct WikipediaResult
    {
        public QueryWikiRes query;
        public ParsedWikiRes parse;
        public string content;
        public string imageURL;
    }

    public class QueryWikiRes
    {
        // This is dynamically typed with objects named after their page IDs, so can't just be a static array
        // See: http://stackoverflow.com/questions/8738031/deserializing-json-using-json-net-with-dynamic-data
        public IDictionary<string, QueryWikiInfo> pages {get; set;}
    }

    public struct QueryWikiInfo
    {
        public string title;
        [JsonProperty("imageinfo")]
        public WikiImg[] imageInfo;
    }

    public struct WikiImg
    {
        public string url;
        [JsonProperty("descriptionurl")]
        public string descriptionUrl;
    }

    public class ParsedWikiRes
    {
        public string title;
        public ParsedWikiHTML text;
        public string[] images;
    }

    public struct ParsedWikiHTML
    {
        [JsonProperty("*")]
        public string HTML;
    }
}