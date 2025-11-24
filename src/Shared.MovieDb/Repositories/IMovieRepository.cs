namespace Shared.MovieDb.Repositories;

using Shared.MovieDb.Models;

/// <summary>
/// Repository contract for movie data access.
/// This interface enables switching of data sources without changing business logic.
/// </summary>
public interface IMovieRepository
{
    /// <summary>
    /// Gets all movies from the data store.
    /// </summary>
    Task<IEnumerable<Movie>> GetAllAsync();

    /// <summary>
    /// Gets a movie by its unique identifier.
    /// </summary>
    /// <param name="id">The movie's unique identifier.</param>
    /// <returns>The movie if found, null otherwise.</returns>
    Task<Movie?> GetByIdAsync(int id);

    /// <summary>
    /// Searches for movies by title (case-insensitive partial match).
    /// </summary>
    /// <param name="title">The title to search for.</param>
    Task<IEnumerable<Movie>> SearchByTitleAsync(string title);

    /// <summary>
    /// Gets movies by genre.
    /// </summary>
    /// <param name="genre">The genre to filter by.</param>
    Task<IEnumerable<Movie>> GetByGenreAsync(string genre);

    /// <summary>
    /// Adds a new movie to the data store.
    /// </summary>
    /// <param name="movie">The movie to add.</param>
    /// <returns>The added movie with its assigned ID.</returns>
    Task<Movie> AddAsync(Movie movie);

    /// <summary>
    /// Updates an existing movie in the data store.
    /// </summary>
    /// <param name="movie">The movie with updated values.</param>
    /// <returns>True if the movie was found and updated, false otherwise.</returns>
    Task<bool> UpdateAsync(Movie movie);

    /// <summary>
    /// Deletes a movie from the data store.
    /// </summary>
    /// <param name="id">The ID of the movie to delete.</param>
    /// <returns>True if the movie was found and deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(int id);
}
