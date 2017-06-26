using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SearchLib
{
    public class Repository
    {
        public static IEnumerable<Movie> GetMoviesFromFile()
        {
            return JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(Settings.MovieJsonFile));
        }
    }
}