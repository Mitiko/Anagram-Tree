using System.Threading.Tasks;
using Anagram_Tree.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Anagram_Tree.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        // GET: Home/Index
        [HttpGet("Index")]
        public IActionResult Index() => View();

        // POST: /Home/Index
        [HttpPost("Index")]
        public async Task<IActionResult> Index(WordViewModel wordViewModel)
        {
            var (data, stats) = await DatabaseScraper.Search(wordViewModel.BaseWord);
            var connections = DatabaseScraper.Setup(ref data);
            stats += DatabaseScraper.PrintStatistics(data, connections);
            if(wordViewModel.RawData)
            {
                stats += DatabaseScraper.PrintAllWords(data);
                return View("RawData", stats);
            }
            else
            {
                return View("GraphVisualization", (
                    stats,
                    DatabaseScraper.PrintDataJson(data),
                    DatabaseScraper.PrintConnectionsJson(data, connections)
                ));
            }
        }
    }
}