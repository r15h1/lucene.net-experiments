using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;

namespace TokenFromAnalyzers
{
    class Program
    {
        private const LuceneVersion MATCH_LUCENE_VERSION= LuceneVersion.LUCENE_48;
        static void Main(string[] args)
        {
            List<Analyzer> analyzers = new List<Analyzer> 
            {
                new KeywordAnalyzer(),
                new WhitespaceAnalyzer(MATCH_LUCENE_VERSION),
                new SimpleAnalyzer(MATCH_LUCENE_VERSION),
                new StopAnalyzer(MATCH_LUCENE_VERSION),
                new StandardAnalyzer(MATCH_LUCENE_VERSION),

                //this was known as the StandardAnalyzer in version 3
                new ClassicAnalyzer(MATCH_LUCENE_VERSION)
            };

            Console.WriteLine("Type string to be analyzed:");
            string userInput = Console.ReadLine();     
            foreach(var analyzer in analyzers)       
                Analyze(analyzer, userInput);
        }

        static void Analyze(Analyzer analyzer, string userInput) 
        {
            Console.WriteLine(analyzer.GetType().Name);
            TokenStream tokenStream = analyzer.GetTokenStream("input", userInput);
            IOffsetAttribute offsetAttribute = tokenStream.AddAttribute<IOffsetAttribute>();
            ICharTermAttribute termAttribute = tokenStream.AddAttribute<ICharTermAttribute>();
            ITypeAttribute typeAttribute = tokenStream.AddAttribute<ITypeAttribute>();
            
            tokenStream.Reset();
            while (tokenStream.IncrementToken()) {
                Console.WriteLine($"{offsetAttribute.StartOffset}-{offsetAttribute.EndOffset}\t{termAttribute.ToString()}\t{typeAttribute.ToString()}"); 
            }
            Console.WriteLine("\n");
        }
    }
}
