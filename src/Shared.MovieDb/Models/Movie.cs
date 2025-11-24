namespace Shared.MovieDb.Models;

/// <summary>
/// Represents a movie in the database.
/// This is a pure data container, free of logic about storage or APIs.
/// </summary>
public class Movie
{
    /// <summary>
    /// Unique identifier for the movie.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the movie.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The year the movie was released.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// The genre of the movie (e.g., "Action", "Comedy", "Drama").
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// The director of the movie.
    /// </summary>
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// A brief description or synopsis of the movie.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The rating of the movie (e.g., 1-10).
    /// </summary>
    public double Rating { get; set; }
}
