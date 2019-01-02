using System.Collections.Generic;
using System.Linq;
using Anagram_Tree.Models;

namespace Anagram_Tree.Models
{
    public static class DataExtensions
    {
        public static Data ToData(this Word word)
        {
            return new Data()
            {
                Id = word.Id,
                Level = word.Name.Length,
                Value = word.Name,
                LinksForward = new List<Data>(),
                LinksBackward = new List<Data>(),
                Power = 0
            };
        }

        public static bool Contains(this Data d1, Data d2)
        {
            // d1 is bigger
            // Setup
            var w = d1.Value.Distinct().ToList();
            var dict = new Dictionary<char, int>();
            w.Sort();
            w.ForEach(l => dict.Add(l, d1.Value.Count(x => x == l)));

            // Execute
            var letters = d2.Value.Distinct().ToList();
            foreach (var l in letters)
            {
                if(!dict.ContainsKey(l))
                    return false;

                if(!(d2.Value.Count(x => x == l) <= dict[l]))
                    return false;
            }

            return true;
        }

        public static bool SanityCheck(this Word word, string original)
        {
            // Setup
            var w = original.Distinct().ToList();
            var dict = new Dictionary<char, int>();
            w.Sort();
            w.ForEach(l => dict.Add(l, original.Count(x => x == l)));

            // Execute
            var letters = word.Name.Distinct().ToList();
            foreach (var l in letters)
            {
                if(!dict.ContainsKey(l))
                    return false;

                if(!(word.Name.Count(x => x == l) <= dict[l]))
                    return false;
            }

            return true;
        }
    }
}