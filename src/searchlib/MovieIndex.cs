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

        private Analyzer SetupAnalyzer()
        {
            return Analyzer.NewAnonymous((field, reader) =>
            {
                var tokenizer = new StandardTokenizer(MATCH_LUCENE_VERSION, reader);
                TokenStream tokenStream = new ASCIIFoldingFilter(tokenizer);
                tokenStream = new LowerCaseFilter(MATCH_LUCENE_VERSION, tokenStream);
                tokenStream = new StopFilter(MATCH_LUCENE_VERSION, tokenStream, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
                return new TokenStreamComponents(tokenizer, tokenStream);
            });
        }

        private QueryParser SetupQueryParser(Analyzer analyzer)
        {
            return new MultiFieldQueryParser(
                MATCH_LUCENE_VERSION,
                new[] { "title", "article", "description" },
                analyzer
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

        private Query BuildQuery(string queryString) =>  queryParser.Parse(Sanitize(queryString));
        
        private string Sanitize(string qs)
        {
            string[] removed = { "*", "?", "%", "+" };
            string[] spaces = { "-" };

            foreach (var r in removed) qs = qs.Replace(r, string.Empty);
            foreach (var s in spaces) qs = qs.Replace(s, " ");

            return qs;
        }

        public void Dispose()
        {
            searchManager?.Dispose();
            analyzer?.Dispose();
            writer?.Dispose();
        }
    }
}