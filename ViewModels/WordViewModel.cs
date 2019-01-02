using System.ComponentModel.DataAnnotations;

namespace Anagram_Tree.ViewModels
{
    public class WordViewModel
    {
        [Required]
        public string BaseWord { get; set; }
        public string ConnectionString { get; set; }
        public bool RawData { get; set; }
    }
}