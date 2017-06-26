using SearchLib;
using System;
using System.IO;

namespace CLI
{
    class Program
    {
        /// <summary>
        /// deletes all the existing index files
        /// then rebuilds the index and prompts user to search
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            DeleteIndexFiles();
            SearchEngine engine = new SearchEngine();
            engine.BuildIndex();
            string userInput = string.Empty;
            Console.WriteLine("type quit to exit");

            do
            {
                Console.Write("\nsearch:\\>");
                userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput)) 
                    continue;
                else if(userInput.Equals("quit")) 
                    break;

                var results = engine.Search(userInput);
                DisplayResults(results);
            } while (true);
        }

        private static void DeleteIndexFiles()
        {
            foreach (FileInfo f in new DirectoryInfo(Settings.IndexLocation).GetFiles())
                f.Delete();
        }

        private static void DisplayResults(SearchResults searchResults)
        {
            Console.WriteLine($"displaying {searchResults.Hits.Count} of {searchResults.TotalHits} results\n------------------------------------------");
            foreach (var result in searchResults.Hits)
                Console.WriteLine($"{result.Title}, {result.Rating} ({result.Score}) {(string.IsNullOrWhiteSpace(result.Snippet) ? "" : "\n" + result.Snippet)}\n");
        }
    }
}