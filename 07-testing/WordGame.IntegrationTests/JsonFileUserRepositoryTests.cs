using WordGame.Api;

namespace WordGame.IntegrationTests;

// #region file
// Integration test with the file system: the repository is tested against a real file
// instead of a fake, because reading and writing the file is exactly what it does.
public class JsonFileUserRepositoryTests : IDisposable
{
    // Every test gets its own temporary file, deleted after the test
    private readonly string _file = Path.Combine(Path.GetTempPath(), $"wordgame-users-{Guid.NewGuid()}.json");

    public void Dispose() => File.Delete(_file);

    [Fact]
    public void FindByEmail_NoUsersSaved_ReturnsNull()
    {
        var repository = new JsonFileUserRepository(_file);

        Assert.Null(repository.FindByEmail("player@test.com"));
    }

    [Fact]
    public void AddOrUpdate_SavedUser_IsReadBackByNewInstance()
    {
        new JsonFileUserRepository(_file).AddOrUpdate(new User { Email = "player@test.com", Experience = 30 });

        // A new instance has nothing in memory, so the user can only come from the file
        var user = new JsonFileUserRepository(_file).FindByEmail("player@test.com");

        Assert.Equal(30, user!.Experience);
    }
    // #endregion file

    [Fact]
    public void AddOrUpdate_ExistingUser_ReplacesIt()
    {
        var repository = new JsonFileUserRepository(_file);
        repository.AddOrUpdate(new User { Email = "player@test.com", Experience = 30 });

        repository.AddOrUpdate(new User { Email = "player@test.com", Experience = 60 });

        Assert.Equal(60, repository.FindByEmail("player@test.com")!.Experience);
    }
}
