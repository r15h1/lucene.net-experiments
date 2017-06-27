using System;
using System.Collections.Generic;
using System.Text;

namespace SearchLib
{
    /// <summary>
    /// domain representation of movie
    /// </summary>
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; }
        public List<Actor> Actors { get; set; } = new List<Actor>();
        public List<Category> Categories { get; set; } =new List<Category>();
    }

    public class Actor
    {
        public int ActorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}