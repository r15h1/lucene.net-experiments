using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;

namespace SearchLib
{
    public class EnhancedEnglishAnalyzer : StopwordAnalyzerBase
    {
        public EnhancedEnglishAnalyzer(LuceneVersion matchVersion, CharArraySet stopwords):base(matchVersion, stopwords) {}
        public EnhancedEnglishAnalyzer(LuceneVersion matchVersion, TextReader stopwords):base(matchVersion, LoadStopwordSet(stopwords, matchVersion)) {}

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer source = new StandardTokenizer(m_matchVersion, reader);
            TokenStream result = new StandardFilter(m_matchVersion, source);
            result = new EnglishPossessiveFilter(m_matchVersion, result);
            result = new ASCIIFoldingFilter(result);
            result = new LowerCaseFilter(m_matchVersion, result);
            result = new StopFilter(m_matchVersion, result, m_stopwords);
            result = new PorterStemFilter(result);
            return new TokenStreamComponents(source, result);
        }
    }

}