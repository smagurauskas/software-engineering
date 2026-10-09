using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WordGame.Api;

namespace WordGame.Testing;

// #region factory
// Starts the real application (Program.cs) in memory, with the parts that tests
// cannot control replaced: the users file, the random word picker and the clock.
public class WordGameFactory : WebApplicationFactory<Program>
{
    // A Monday, so the weekend bonus does not apply unless a test moves the clock
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero));

    // A separate file for every factory, so test runs never share users
    public string UsersFile { get; } = Path.Combine(Path.GetTempPath(), $"wordgame-users-{Guid.NewGuid()}.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("UsersFile", UsersFile);

        // Runs after Program.cs registrations, so these replace the real ones
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IWordPicker>(new FixedWordPicker("hello", "world", "game"));
            services.AddSingleton<TimeProvider>(Time);
        });
    }
    // #endregion factory

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        File.Delete(UsersFile);
    }
}
