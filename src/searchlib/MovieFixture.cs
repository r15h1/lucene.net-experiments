using System;

namespace SearchLib
{
    public class MovieFixture : IDisposable
    {
        private MovieIndex movieIndex;

        public MovieFixture() { 
            PrepareTestIndex();
        }

        public MovieIndex MovieIndex{
            get{
                return movieIndex;
            }
        }

        private void PrepareTestIndex(){
            var movies = new Movie[]{
                new Movie{ MovieId = 1, Title = "Living Water" },
                new Movie{ MovieId = 2, Title = "Ring of Bright Water" },
                new Movie{ MovieId = 3, Title = "Fish Out of Water" },
                new Movie{ MovieId = 4, Title = "Woman on Fire Looks for Water" },
                new Movie{ MovieId = 5, Title = "Promises Written in Water" },
                new Movie{ MovieId = 6, Title = "House of Wax" },
                new Movie{ MovieId = 7, Title = "Promises and Lies" },
                new Movie{ MovieId = 8, Title = "House on Haunted Hill" },
                new Movie{ MovieId = 9, Title = "Home Alone" },
                new Movie{ MovieId = 10, Title = "Home Sweet Home" }
            };

            movieIndex = new MovieIndex(@"C:\Temp\Index");
            movieIndex.BuildIndex(movies);
        }

        public void Dispose() => movieIndex.Dispose();
    }
}