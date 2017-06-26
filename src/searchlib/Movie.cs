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
        public Movie()
        {
            Actors = new List<Actor>();
            Categories = new List<Category>();
        }

        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; }
        public List<Actor> Actors { get; set; }
        public List<Category> Categories { get; set; }
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