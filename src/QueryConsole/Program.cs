using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using MovieDatabase;

namespace QueryConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Setting up index location");
            string indexLocation = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}/movie_index";
            var index = SetupIndex(indexLocation);            

            Console.WriteLine($"Preparing search console");
            string input = null;
            SearchResults results = null;
            Stopwatch stopWatch = new Stopwatch();
            do {
                Console.Write("\nsearch:>");
                stopWatch.Reset();
                input = Console.ReadLine();
                stopWatch.Start();
                results = index.Search(input);
                PrintResults(results);
                stopWatch.Stop();
                Console.WriteLine($"time: {stopWatch.Elapsed.Milliseconds} ms");

            } while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase));
        }

        private static MovieIndex SetupIndex (string indexLocation)
        {
            if (!Directory.Exists(indexLocation)) {
                Directory.CreateDirectory(indexLocation);
            }
            else {
                foreach (FileInfo f in new DirectoryInfo(indexLocation).GetFiles())
                    f.Delete();            
            }

            Repository repo = new Repository();
            IEnumerable<Movie> movies = repo.GetMovies();

            Console.WriteLine($"Creating index for {movies.Count()} movies");
            MovieIndex index = new MovieIndex(indexLocation);
            index.Build(movies);

            return index;
        }

        private static void PrintResults(SearchResults results) 
        {
            int index = 0;
            foreach(var result in results.Hits) {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"({++index}) {result.Year}: {result.Title}");                
                Console.ForegroundColor = originalColor;
                Console.WriteLine($"CAST: {result.Cast}");                
                Console.WriteLine($"GENRE: {(string.IsNullOrWhiteSpace(result.Genres) ? "Unknown" : result.Genres)}"); 
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"score: {result.Score}\n");
                Console.ForegroundColor = originalColor;
            }

            Console.WriteLine($"{results.TotalHits} results found");
        }
    }
}
