using System;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Xunit;

namespace SearchLib
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
    }
}
