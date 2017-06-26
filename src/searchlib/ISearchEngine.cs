namespace SearchLib
{
    public interface ISearchEngine
    {
        void BuildIndex();
        SearchResults Search(string query);
    }
}