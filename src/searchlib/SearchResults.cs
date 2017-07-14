using System;
using System.Collections.Generic;

namespace SearchLib
{
    /// <summary>
    /// results after executing a lucene.net search on the movie index
    /// </summary>
    public class SearchResults
    {
        public SearchResults() => Hits = new List<Hit>();
        public string Time { get; set; }
        public int TotalHits { get; set; }
        public IList<Hit> Hits { get; set; }
    }

    /// <summary>
    /// a representation of a movie item retrieved from lucene.net
    /// </summary>
    public class Hit {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public float Score { get; set; }
    }
}
