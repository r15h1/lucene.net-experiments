using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Xunit;


namespace SearchLib
{
    public class AnalyzerTests
    {
        private const LuceneVersion MATCH_LUCENE_VERSION= LuceneVersion.LUCENE_48;
        
        [Fact]
        public void TestTokenization()
        {
            var stdAnalyzer = new StandardAnalyzer(MATCH_LUCENE_VERSION, StandardAnalyzer.STOP_WORDS_SET);
            var engAnalyzer = new EnglishAnalyzer(MATCH_LUCENE_VERSION, EnglishAnalyzer.DefaultStopSet);
            var frAnalyzer = new FrenchAnalyzer(MATCH_LUCENE_VERSION, FrenchAnalyzer.DefaultStopSet);

            var stdList = Tokenize("My friends are visiting Montréal's engineering institutions", stdAnalyzer);        
            var engList = Tokenize("My friends are visiting Montréal's engineering institutions", engAnalyzer);
            var frList = Tokenize("Mes amis visitent les instituts d'ingénierie de Montréal", frAnalyzer);
        }        

        private IEnumerable<string> Tokenize(string text, Analyzer analyzer)
        {
            var tokens = new List<string>();
            using(var reader = new System.IO.StringReader(text))
            using(TokenStream stream = analyzer.GetTokenStream(null, reader))
            {
                stream.Reset();
                while(stream.IncrementToken())                
                    tokens.Add(stream.GetAttribute<ICharTermAttribute>().ToString());
            }
            return tokens;
        }
    }
}