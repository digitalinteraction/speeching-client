using System;
using System.Collections.Generic;
using System.Text;

namespace Droid_PeopleWithParkinsons
{
    class PlaceholderStrings
    {
        private static Random random;
        private static string[] exampleSentences = 
        {
            "\"This is an example string\"",
            "\"This is a longer example string\"",
            "\"This is a much longer example string\"",
            "\"This is a very much longer example string\"",
            "\"This example string length is getting ridiculous now\"",
            "\"This example string length needs to be even more ridiculous\"",
            "\"This example string length is so long now that it might just overflow the box\"",
            "\"This is the maximum length that a string can be before it starts to overflow the text box.....W\""
            // Note: Currently not using a monospaced font?
            // TODO: Check monospace fonting
            // Max characters for text 95-99 chars?

        };

        public static string GetRandomSentence()
        {
            if (random == null)
            {
                random = new Random();
            }

            return exampleSentences[random.Next(0, exampleSentences.Length)];
        }
    }
}
