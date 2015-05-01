using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingCommon
{
    public class Guide : SpeechingActivityItem
    {
        public struct Page
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            [ForeignKey(typeof(Guide))]
            public int GuideId { get; set; }
            public string MediaLocation { get; set; }
            public string Text { get; set; }
        };

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public Page[] Guides { get; set; }
    }
}