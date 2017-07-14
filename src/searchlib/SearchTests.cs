using System;
using Lucene.Net.Index;
using Lucene.Net.Search;
using SearchLib;
using Xunit;

namespace searchtest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var index = new MovieIndex(@"C:\Temp\Index");
            Query q = new PrefixQuery(new Term("title", "home"));
            var results = index.Search(q);
        }
    }
}
