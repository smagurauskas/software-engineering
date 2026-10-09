using WordGame.Api;

namespace WordGame.UnitTests;

// #region fake
// Fake: a working implementation that keeps users in memory instead of a file.
// Each test creates its own instance, so no state is shared between tests.
public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _users = new();

    public User? FindByEmail(string email) => _users.GetValueOrDefault(email);

    public void AddOrUpdate(User user) => _users[user.Email] = user;
}
// #endregion
