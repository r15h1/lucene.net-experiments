using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;

namespace MovieDatabase
{
    public class MovieIndex : IDisposable
    {
        private const LuceneVersion MATCH_LUCENE_VERSION= LuceneVersion.LUCENE_48;
        private readonly IndexWriter writer;
        private readonly Analyzer analyzer;
        private readonly QueryParser queryParser;
        private readonly SearcherManager searchManager;

        public MovieIndex(string indexPath)
        {            
            analyzer = SetupAnalyzer();
            queryParser = SetupQueryParser(analyzer);
            writer = new IndexWriter(FSDirectory.Open(indexPath), new IndexWriterConfig(MATCH_LUCENE_VERSION, analyzer));
            searchManager = new SearcherManager(writer, true, null);
        }

        private Analyzer SetupAnalyzer() => new StandardAnalyzer(MATCH_LUCENE_VERSION);

        private QueryParser SetupQueryParser(Analyzer analyzer) => new QueryParser(MATCH_LUCENE_VERSION, "title", analyzer);

        public void Build(IEnumerable<Movie> movies)
        {
            if (movies == null) throw new ArgumentNullException();

            foreach (var movie in movies)
                writer.AddDocument(BuildDocument(movie));

            writer.Flush(true, true);
            writer.Commit();
        }

        private Document BuildDocument(Movie movie)
        {
            Document doc = new Document
            {                
                new TextField("title", movie.Title, Field.Store.YES),
                new StringField("year", movie.Year.ToString(), Field.Store.YES),
                new TextField("cast", string.Join(", ", movie.Cast), Field.Store.YES), 
                new TextField("genres", string.Join(", ", movie.Genres), Field.Store.YES)
            };

            return doc;
        }

        public SearchResults Search(string queryString)
        {
            int resultsPerPage = 100;
            Query query = queryParser.Parse(queryString);
            Console.WriteLine($"query in lucene syntax => {query.ToString()} \n");
            searchManager.MaybeRefreshBlocking();
            IndexSearcher searcher = searchManager.Acquire();

            try
            {
                TopDocs topdDocs = searcher.Search(query, resultsPerPage);                
                return CompileResults(searcher, topdDocs);
            }
            finally
            {
                searchManager.Release(searcher);
                searcher = null;
            }
        }

        private SearchResults CompileResults(IndexSearcher searcher, TopDocs topdDocs)
        {
            SearchResults searchResults = new SearchResults() { TotalHits = topdDocs.TotalHits };
            foreach (var result in topdDocs.ScoreDocs)
            {
                Document document = searcher.Doc(result.Doc);
                Hit searchResult = new Hit
                {
                    Title = document.GetField("title")?.GetStringValue(),
                    Year = document.GetField("year")?.GetStringValue(),
                    Cast = document.GetField("cast")?.GetStringValue(),
                    Score = result.Score,
                    Genres = document.GetField("genres")?.GetStringValue()
                };

                searchResults.Hits.Add(searchResult);
            }

            return searchResults;
        }
        public void Dispose()
        {
            searchManager?.Dispose();
            analyzer?.Dispose();
            writer?.Dispose();
        }
    }
}