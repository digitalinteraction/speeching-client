using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
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
        public QueryWikiData pages;
    }

    public struct QueryWikiData
    {
        [JsonProperty("-1")]
        public QueryWikiInfo info;
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
        // I am angry at whoever though an asterix was a good variable name
        [JsonProperty("*")]
        public string HTML;
    }
}