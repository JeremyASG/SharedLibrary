namespace Shared.MovieDb.Tests;

using Shared.MovieDb.Models;
using Shared.MovieDb.Repositories;
using Shared.MovieDb.Services;

public class MovieServiceTests
{
    private readonly MovieService _service;
    private readonly InMemoryMovieRepository _repository;

    public MovieServiceTests()
    {
        _repository = new InMemoryMovieRepository();
        _service = new MovieService(_repository);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ReturnsSuccess()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "Test Movie", Year = 2020 });

        // Act
        var result = await _service.GetAllMoviesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ReturnsSuccess_WhenMovieExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "Test Movie", Year = 2020 });

        // Act
        var result = await _service.GetMovieByIdAsync(movie.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Movie", result.Data?.Title);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ReturnsFailure_WhenMovieNotExists()
    {
        // Act
        var result = await _service.GetMovieByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ReturnsFailure_WhenIdIsInvalid()
    {
        // Act
        var result = await _service.GetMovieByIdAsync(0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("greater than zero", result.Error);
    }

    [Fact]
    public async Task SearchMoviesAsync_ReturnsSuccess_WithMatchingMovies()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "The Matrix", Year = 1999 });
        await _repository.AddAsync(new Movie { Title = "Inception", Year = 2010 });

        // Act
        var result = await _service.SearchMoviesAsync("Matrix");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task SearchMoviesAsync_ReturnsFailure_WhenTitleIsEmpty()
    {
        // Act
        var result = await _service.SearchMoviesAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("empty", result.Error);
    }

    [Fact]
    public async Task AddMovieAsync_ReturnsSuccess_WithValidMovie()
    {
        // Arrange
        var movie = new Movie
        {
            Title = "New Movie",
            Year = 2023,
            Genre = "Drama",
            Rating = 8.0
        };

        // Act
        var result = await _service.AddMovieAsync(movie);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data?.Id > 0);
    }

    [Fact]
    public async Task AddMovieAsync_ReturnsFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var movie = new Movie { Title = "", Year = 2023 };

        // Act
        var result = await _service.AddMovieAsync(movie);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("title is required", result.Error);
    }

    [Fact]
    public async Task AddMovieAsync_ReturnsFailure_WhenYearIsTooOld()
    {
        // Arrange
        var movie = new Movie { Title = "Ancient Movie", Year = 1800 };

        // Act
        var result = await _service.AddMovieAsync(movie);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("1888", result.Error);
    }

    [Fact]
    public async Task AddMovieAsync_ReturnsFailure_WhenRatingIsInvalid()
    {
        // Arrange
        var movie = new Movie { Title = "Test", Year = 2020, Rating = 15 };

        // Act
        var result = await _service.AddMovieAsync(movie);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("between 0 and 10", result.Error);
    }

    [Fact]
    public async Task UpdateMovieAsync_ReturnsSuccess_WhenMovieExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "Original", Year = 2020 });
        movie.Title = "Updated";

        // Act
        var result = await _service.UpdateMovieAsync(movie);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateMovieAsync_ReturnsFailure_WhenMovieNotExists()
    {
        // Arrange
        var movie = new Movie { Id = 999, Title = "Nonexistent", Year = 2020 };

        // Act
        var result = await _service.UpdateMovieAsync(movie);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task DeleteMovieAsync_ReturnsSuccess_WhenMovieExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "To Delete", Year = 2020 });

        // Act
        var result = await _service.DeleteMovieAsync(movie.Id);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteMovieAsync_ReturnsFailure_WhenMovieNotExists()
    {
        // Act
        var result = await _service.DeleteMovieAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_ReturnsSuccess()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "Action Movie", Year = 2020, Genre = "Action" });
        await _repository.AddAsync(new Movie { Title = "Comedy Movie", Year = 2020, Genre = "Comedy" });

        // Act
        var result = await _service.GetMoviesByGenreAsync("Action");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_ReturnsFailure_WhenGenreIsEmpty()
    {
        // Act
        var result = await _service.GetMoviesByGenreAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("empty", result.Error);
    }
}
