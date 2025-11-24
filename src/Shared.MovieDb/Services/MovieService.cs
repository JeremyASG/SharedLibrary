namespace Shared.MovieDb.Services;

using Shared.MovieDb.Models;
using Shared.MovieDb.Repositories;

/// <summary>
/// Service layer for movie business logic.
/// Separates what the program does (business logic) from how it stores data.
/// </summary>
public class MovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets all movies in the database.
    /// </summary>
    public async Task<Result<IEnumerable<Movie>>> GetAllMoviesAsync()
    {
        var movies = await _repository.GetAllAsync();
        return Result<IEnumerable<Movie>>.Success(movies);
    }

    /// <summary>
    /// Gets a movie by its ID.
    /// </summary>
    public async Task<Result<Movie>> GetMovieByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<Movie>.Failure("Movie ID must be greater than zero.");
        }

        var movie = await _repository.GetByIdAsync(id);
        if (movie == null)
        {
            return Result<Movie>.Failure($"Movie with ID {id} not found.");
        }

        return Result<Movie>.Success(movie);
    }

    /// <summary>
    /// Searches for movies by title.
    /// </summary>
    public async Task<Result<IEnumerable<Movie>>> SearchMoviesAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<IEnumerable<Movie>>.Failure("Search title cannot be empty.");
        }

        var movies = await _repository.SearchByTitleAsync(title);
        return Result<IEnumerable<Movie>>.Success(movies);
    }

    /// <summary>
    /// Gets movies by genre.
    /// </summary>
    public async Task<Result<IEnumerable<Movie>>> GetMoviesByGenreAsync(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
        {
            return Result<IEnumerable<Movie>>.Failure("Genre cannot be empty.");
        }

        var movies = await _repository.GetByGenreAsync(genre);
        return Result<IEnumerable<Movie>>.Success(movies);
    }

    /// <summary>
    /// Adds a new movie to the database.
    /// </summary>
    public async Task<Result<Movie>> AddMovieAsync(Movie movie)
    {
        var validationResult = ValidateMovie(movie);
        if (!validationResult.IsSuccess)
        {
            return Result<Movie>.Failure(validationResult.Error!);
        }

        var addedMovie = await _repository.AddAsync(movie);
        return Result<Movie>.Success(addedMovie);
    }

    /// <summary>
    /// Updates an existing movie.
    /// </summary>
    public async Task<Result> UpdateMovieAsync(Movie movie)
    {
        if (movie.Id <= 0)
        {
            return Result.Failure("Movie ID must be greater than zero.");
        }

        var validationResult = ValidateMovie(movie);
        if (!validationResult.IsSuccess)
        {
            return Result.Failure(validationResult.Error!);
        }

        var updated = await _repository.UpdateAsync(movie);
        if (!updated)
        {
            return Result.Failure($"Movie with ID {movie.Id} not found.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Deletes a movie by its ID.
    /// </summary>
    public async Task<Result> DeleteMovieAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Movie ID must be greater than zero.");
        }

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return Result.Failure($"Movie with ID {id} not found.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates movie data before add or update operations.
    /// </summary>
    private static Result ValidateMovie(Movie movie)
    {
        if (string.IsNullOrWhiteSpace(movie.Title))
        {
            return Result.Failure("Movie title is required.");
        }

        if (movie.Year < 1888) // First movie ever made
        {
            return Result.Failure("Movie year must be 1888 or later.");
        }

        if (movie.Year > DateTime.Now.Year + 5) // Allow movies up to 5 years in the future
        {
            return Result.Failure($"Movie year cannot be more than 5 years in the future.");
        }

        if (movie.Rating < 0 || movie.Rating > 10)
        {
            return Result.Failure("Movie rating must be between 0 and 10.");
        }

        return Result.Success();
    }
}
