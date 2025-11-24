namespace Shared.MovieDb.Tests;

using Shared.MovieDb.Models;
using Shared.MovieDb.Repositories;

public class InMemoryMovieRepositoryTests
{
    private readonly InMemoryMovieRepository _repository;

    public InMemoryMovieRepositoryTests()
    {
        _repository = new InMemoryMovieRepository();
    }

    [Fact]
    public async Task AddAsync_AssignsIdAndReturnsMovie()
    {
        // Arrange
        var movie = new Movie
        {
            Title = "The Matrix",
            Year = 1999,
            Genre = "Sci-Fi",
            Director = "The Wachowskis",
            Rating = 8.7
        };

        // Act
        var result = await _repository.AddAsync(movie);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("The Matrix", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMovie_WhenExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "Test Movie" });

        // Act
        var result = await _repository.GetByIdAsync(movie.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movie.Id, result.Id);
        Assert.Equal("Test Movie", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "Movie 1" });
        await _repository.AddAsync(new Movie { Title = "Movie 2" });
        await _repository.AddAsync(new Movie { Title = "Movie 3" });

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task SearchByTitleAsync_ReturnsMatchingMovies()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "The Matrix" });
        await _repository.AddAsync(new Movie { Title = "The Matrix Reloaded" });
        await _repository.AddAsync(new Movie { Title = "Inception" });

        // Act
        var result = await _repository.SearchByTitleAsync("Matrix");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByGenreAsync_ReturnsMoviesInGenre()
    {
        // Arrange
        await _repository.AddAsync(new Movie { Title = "Movie 1", Genre = "Action" });
        await _repository.AddAsync(new Movie { Title = "Movie 2", Genre = "Comedy" });
        await _repository.AddAsync(new Movie { Title = "Movie 3", Genre = "Action" });

        // Act
        var result = await _repository.GetByGenreAsync("Action");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.Equal("Action", m.Genre));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenMovieExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "Original Title" });
        movie.Title = "Updated Title";

        // Act
        var result = await _repository.UpdateAsync(movie);

        // Assert
        Assert.True(result);
        var updated = await _repository.GetByIdAsync(movie.Id);
        Assert.Equal("Updated Title", updated?.Title);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenMovieNotExists()
    {
        // Act
        var result = await _repository.UpdateAsync(new Movie { Id = 999, Title = "Test" });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenMovieExists()
    {
        // Arrange
        var movie = await _repository.AddAsync(new Movie { Title = "To Delete" });

        // Act
        var result = await _repository.DeleteAsync(movie.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _repository.GetByIdAsync(movie.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenMovieNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }
}
