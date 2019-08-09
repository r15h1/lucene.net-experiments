using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MovieDatabase
{
    public class Repository
    {
        public IEnumerable<Movie> GetMovies()
        {
            string movieFile = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}/movies.json";
            return JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(movieFile));
        }
    }
}
