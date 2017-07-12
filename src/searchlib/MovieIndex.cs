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
using System.Text.RegularExpressions;
using Lucene.Net.Search.Spans;

namespace SearchLib
{
    /// <summary>
    /// movies indexed in lucene.net
    /// </summary>
    internal class MovieIndex : IDisposable
    {
        private const LuceneVersion MATCH_LUCENE_VERSION= LuceneVersion.LUCENE_48;
        private const int SNIPPET_LENGTH = 100;
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

        private Analyzer SetupAnalyzer() => new StandardAnalyzer(MATCH_LUCENE_VERSION, StandardAnalyzer.STOP_WORDS_SET);
 
        private QueryParser SetupQueryParser(Analyzer analyzer)
        {
            return new MultiFieldQueryParser(
                MATCH_LUCENE_VERSION,
                new[] { "title", "description" },
                analyzer,
                new Dictionary<string, float>{{"title", 3f}, {"description", 0.001f}}
            );
        }

        public void BuildIndex(IEnumerable<Movie> movies)
        {
            if (movies == null) throw new ArgumentNullException();

            foreach (var movie in movies)            
            {
                Document movieDocument = BuildDocument(movie);
                writer.UpdateDocument(new Term("id", movie.MovieId.ToString()), movieDocument);
            }                

            writer.Flush(true, true);
            writer.Commit();
        }

        private Document BuildDocument(Movie movie)
        {
            Document doc = new Document
            {
                new StoredField("movieid", movie.MovieId),
                new TextField("title", movie.Title, Field.Store.YES),
                new TextField("description", movie.Description, Field.Store.NO),
                new StoredField("snippet", MakeSnippet(movie.Description)),
                new StringField("rating", movie.Rating, Field.Store.YES)
            };

            return doc;
        }

        private string MakeSnippet(string description)
        {
            return (string.IsNullOrWhiteSpace(description) || description.Length <= SNIPPET_LENGTH)
                    ? description 
                    : $"{description.Substring(0, SNIPPET_LENGTH)}...";
        }

        public SearchResults Search(string queryString)
        {
            int resultsPerPage = 10;
            Query query = BuildQuery(queryString);
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

        private Query BuildQuery(string queryString) {                        
            PrefixQuery pq = new PrefixQuery(new Term("title", queryString));
            return pq;
        }

        private string Sanitize(string queryString) {
            string toBeReplaced = @"[\+-&|!(){}[]^~*?:/]";            
            string sanitized = queryString.ToLowerInvariant();
            for(int i = 0; i < toBeReplaced.Length; i++)
                sanitized = sanitized.Replace(toBeReplaced.Substring(i,1), string.Empty);

            return sanitized;
        }

        private SearchResults CompileResults(IndexSearcher searcher, TopDocs topdDocs)
        {
            SearchResults searchResults = new SearchResults() { TotalHits = topdDocs.TotalHits };
            foreach (var result in topdDocs.ScoreDocs)
            {
                Document document = searcher.Doc(result.Doc);
                Hit searchResult = new Hit
                {
                    Rating = document.GetField("rating")?.GetStringValue(),
                    MovieId = document.GetField("movieid")?.GetStringValue(),
                    Score = result.Score,
                    Title = document.GetField("title")?.GetStringValue(),
                    Snippet = document.GetField("snippet")?.GetStringValue()
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