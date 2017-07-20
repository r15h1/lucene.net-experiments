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
using System.IO;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Fr;

namespace SearchLib
{
    /// <summary>
    /// movies indexed in lucene.net
    /// </summary>
    public class MovieIndex : IDisposable
    {
        private const LuceneVersion MATCH_LUCENE_VERSION= LuceneVersion.LUCENE_48;
        private const int SNIPPET_LENGTH = 100;
        private readonly IndexWriter writer;
        private readonly Analyzer stdAnalyzer, engAnalyzer, frAnalyzer;
        private readonly SearcherManager searchManager;

        public MovieIndex(string indexPath)
        {            
            stdAnalyzer = new StandardAnalyzer(MATCH_LUCENE_VERSION, StandardAnalyzer.STOP_WORDS_SET);
            engAnalyzer = new EnglishAnalyzer(MATCH_LUCENE_VERSION, EnglishAnalyzer.DefaultStopSet);
            frAnalyzer = new FrenchAnalyzer(MATCH_LUCENE_VERSION, FrenchAnalyzer.DefaultStopSet);

            writer = new IndexWriter(FSDirectory.Open(indexPath), new IndexWriterConfig(MATCH_LUCENE_VERSION, stdAnalyzer){ OpenMode = OpenMode.CREATE_OR_APPEND });
            searchManager = new SearcherManager(writer, true, null);
        }

        private Analyzer SetupAnalyzer() => new StandardAnalyzer(MATCH_LUCENE_VERSION, StandardAnalyzer.STOP_WORDS_SET);
        //private Analyzer SetupAnalyzer() => new EnglishAnalyzer(MATCH_LUCENE_VERSION, StandardAnalyzer.STOP_WORDS_SET);
 
        public void BuildIndex(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
                writer.UpdateDocument(new Term("id", movie.MovieId.ToString()), BuildDocument(movie));

            writer.Flush(true, true);
            writer.Commit();
        }

        private Document BuildDocument(Movie movie)
        {
            return new Document
            {
                new TextField("title", movie.Title, Field.Store.YES)        
            };
        }

        private string MakeSnippet(string description)
        {
            return (string.IsNullOrWhiteSpace(description) || description.Length <= SNIPPET_LENGTH)
                    ? description 
                    : $"{description.Substring(0, SNIPPET_LENGTH)}...";
        }

        public SearchResults Search(Query query)
        {
            int resultsPerPage = 10;            
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

        public IList<string> Tokenize(string text)
        {
            var tokens = new List<string>();
            using(var reader = new StringReader(text))
            using(TokenStream stream = stdAnalyzer.GetTokenStream(null, reader))
            {
                stream.Reset();
                while(stream.IncrementToken())                
                    tokens.Add(stream.GetAttribute<ICharTermAttribute>().ToString());
            }

            return tokens;
        }

        private SearchResults CompileResults(IndexSearcher searcher, TopDocs topdDocs)
        {
            SearchResults searchResults = new SearchResults() { TotalHits = topdDocs.TotalHits };
            foreach (var result in topdDocs.ScoreDocs)
            {
                Document document = searcher.Doc(result.Doc);
                Hit searchResult = new Hit
                {                    
                    Score = result.Score,
                    Title = document.GetField("title")?.GetStringValue()
                };

                searchResults.Hits.Add(searchResult);
            }

            return searchResults;
        }        

        public void Dispose()
        {
            searchManager?.Dispose();
            stdAnalyzer?.Dispose();
            engAnalyzer?.Dispose();
            frAnalyzer?.Dispose();
            writer?.Dispose();
        }
    }
}