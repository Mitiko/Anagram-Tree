using System.Threading.Tasks;
using Anagram_Tree.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Anagram_Tree.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        [HttpPost()]
        public async Task<IActionResult> Index(WordViewModel wordViewModel)
        {
            if(wordViewModel.ConnectionString == null)
                wordViewModel.ConnectionString = "Server=manny.db.elephantsql.com;Port=5432;Database=puoddiwe;User Id=puoddiwe;Password=48FoDgEzGZxZxQIQo3RPvbgJZLR-gdO7;";
            var (data, stats) = await DatabaseScraper.Search(wordViewModel.BaseWord, wordViewModel.ConnectionString);
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