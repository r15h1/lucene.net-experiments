namespace SearchLib
{
    /// <summary>
    /// facade around lucene.net's movie index
    /// </summary>
    public class SearchEngine : ISearchEngine
    {
        private readonly MovieIndex index;

        public SearchEngine()
        {
            index = new MovieIndex(Settings.IndexLocation);
        }

        public void BuildIndex() => index.BuildIndex(Repository.GetMoviesFromFile());

        public SearchResults Search(string query) => index.Search(query);        
    }
}