using System;
using System.Collections.Generic;

namespace MovieDatabase
{

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
        public string Title { get; set; }
        public string Year { get; set; }
        public string Cast { get; set; }
        public string Genres { get; set; }
        public float Score { get; set; }
    }
}