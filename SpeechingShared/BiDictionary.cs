using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechingShared
{
    public class BiDictionary<TFirst, TSecond>
    {
        public IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        public IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }
    }
}