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
            "\"This is a simple example sentence.\"",
            "\"Coming up with some random sentences is harder than you think.\"",
            "\"It's nice to see you to see you nice.\"",
            "\"Did you read the newspaper last night? There was a really good article on chocolate.\"",
            "\"I really liked the long black dress she was wearing.\"",
            "\"There are some great films on at the cinema tonight.\"",
            "\"I'll be back in a few minutes, I'm just going to the shop to buy some eggs and bread.\"",
            "\"It's really cold outside, you should take a coat to keep warm.\"",
            "\"The longer the text, the smaller the text becomes. This way, we can support any length sentence without overflowing the text box.\""
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
