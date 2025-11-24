namespace Shared.MovieDb.Repositories;

using Shared.MovieDb.Models;

/// <summary>
/// In-memory implementation of the movie repository.
/// This implementation hides the details of where the data lives — in this case, an in-memory list.
/// </summary>
public class InMemoryMovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();
    private int _nextId = 1;
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task<IEnumerable<Movie>> GetAllAsync()
    {
        lock (_lock)
        {
            // Return a copy to prevent external modification
            return Task.FromResult<IEnumerable<Movie>>(
                _movies.Select(m => CloneMovie(m)).ToList());
        }
    }

    /// <inheritdoc />
    public Task<Movie?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(movie != null ? CloneMovie(movie) : null);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<Movie>> SearchByTitleAsync(string title)
    {
        lock (_lock)
        {
            var results = _movies
                .Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                .Select(m => CloneMovie(m))
                .ToList();
            return Task.FromResult<IEnumerable<Movie>>(results);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<Movie>> GetByGenreAsync(string genre)
    {
        lock (_lock)
        {
            var results = _movies
                .Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .Select(m => CloneMovie(m))
                .ToList();
            return Task.FromResult<IEnumerable<Movie>>(results);
        }
    }

    /// <inheritdoc />
    public Task<Movie> AddAsync(Movie movie)
    {
        lock (_lock)
        {
            var newMovie = CloneMovie(movie);
            newMovie.Id = _nextId++;
            _movies.Add(newMovie);
            return Task.FromResult(CloneMovie(newMovie));
        }
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Movie movie)
    {
        lock (_lock)
        {
            var index = _movies.FindIndex(m => m.Id == movie.Id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _movies[index] = CloneMovie(movie);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
    {
        lock (_lock)
        {
            var index = _movies.FindIndex(m => m.Id == id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _movies.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Creates a deep copy of a movie to prevent external modification.
    /// </summary>
    private static Movie CloneMovie(Movie movie)
    {
        return new Movie
        {
            Id = movie.Id,
            Title = movie.Title,
            Year = movie.Year,
            Genre = movie.Genre,
            Director = movie.Director,
            Description = movie.Description,
            Rating = movie.Rating
        };
    }
}
