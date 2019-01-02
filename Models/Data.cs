using System.Collections.Generic;
using System.Linq;

namespace Anagram_Tree.Models
{
    public class Data
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Value { get; set; }
        public int Power { get; set; }
        public List<Data> LinksForward { get; set; }
        public List<Data> LinksBackward { get; set; }
    }
}