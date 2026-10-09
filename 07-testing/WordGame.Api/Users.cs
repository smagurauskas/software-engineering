using System.Text.Json;

namespace WordGame.Api;

public class User
{
    public required string Email { get; init; }
    public int Experience { get; set; }
}

public interface IUserRepository
{
    User? FindByEmail(string email);
    void AddOrUpdate(User user);
}

public class JsonFileUserRepository(string filePath) : IUserRepository
{
    private readonly Lock _lock = new();

    public User? FindByEmail(string email)
    {
        lock (_lock)
        {
            return ReadAll().FirstOrDefault(u => u.Email == email);
        }
    }

    public void AddOrUpdate(User user)
    {
        lock (_lock)
        {
            var users = ReadAll();
            users.RemoveAll(u => u.Email == user.Email);
            users.Add(user);
            File.WriteAllText(filePath, JsonSerializer.Serialize(users));
        }
    }

    private List<User> ReadAll() =>
        File.Exists(filePath)
            ? JsonSerializer.Deserialize<List<User>>(File.ReadAllText(filePath)) ?? []
            : [];
}

public class UserService(IUserRepository repository)
{
    public void AddExperience(string email, int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var user = repository.FindByEmail(email) ?? new User { Email = email };
        user.Experience += amount;
        repository.AddOrUpdate(user);
    }

    public User? GetUserByEmail(string email) => repository.FindByEmail(email);
}
