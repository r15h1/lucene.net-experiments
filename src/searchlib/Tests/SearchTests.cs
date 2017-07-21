using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Xunit;

namespace SearchLib.Test
{
    public class SearchTest:IClassFixture<MovieFixture>
    {
        private MovieIndex movieIndex;
        public SearchTest(MovieFixture movieFixture)
        {
            this.movieIndex = movieFixture.MovieIndex;
        }

        [Fact]
        public void Test1()
        {            
            Query q = new PrefixQuery(new Term("title", "home"));
            var results = movieIndex.Search(q);
            Assert.Equal(results.Hits.Count, 2);
        }

        [Fact]
        void TokenTests()
        {               
            var q = BuildQuery("great white shark australia", new List<FieldDefinition>{ new FieldDefinition{ Name = "title", IsDefault = true }, new FieldDefinition{ Name = "description" }});

        }

        Query BuildQuery(string userInput, IEnumerable<FieldDefinition> fields)
        {
            BooleanQuery query = new BooleanQuery();
            IList<string> tokens = movieIndex.Tokenize(userInput);

            //combine tokens present in user input
            if(tokens.Count > 1)
            {            
                FieldDefinition defaultField = fields.FirstOrDefault(f => f.IsDefault == true);                
                query.Add(BuildExactPhraseQuery(tokens, defaultField), Occur.SHOULD);

                foreach(var q in GetIncrementalMatchQuery(tokens, defaultField))
                    query.Add(q, Occur.SHOULD);
            }

            //create a term query per field - non boosted
            foreach(var token in tokens)
                foreach(var field in fields)
                    query.Add(new TermQuery(new Term(field.Name, token)), Occur.SHOULD);            

            return query;
        }

        Query BuildExactPhraseQuery(IList<string> tokens, FieldDefinition field)
        {
            //boost factor (6) and slop (2) come from configuration - code omitted for simplicity
            PhraseQuery pq = new PhraseQuery() { Boost = tokens.Count * 6, Slop = 2 };
            foreach(var token in tokens)
                pq.Add(new Term(field.Name, token));
            
            return pq;
        }

        IEnumerable<Query> GetIncrementalMatchQuery(IList<string> tokens, FieldDefinition field)
        {
            BooleanQuery bq = new BooleanQuery();
            foreach(var token in tokens)
                bq.Add(new TermQuery(new Term(field.Name, token)), Occur.SHOULD);

            //5 comes from config - code omitted
            int upperLimit = Math.Min(tokens.Count, 5);
            for(int match = 2; match <= upperLimit; match++)
            {
                BooleanQuery q = bq.Clone() as BooleanQuery;
                q.Boost = match * 3;
                q.MinimumNumberShouldMatch = match;
                yield return q;
            }
        }

        

    }

    class FieldDefinition
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; } = false;

        //other properties omitted
    }


}