using Microsoft.Extensions.Time.Testing;
using WordGame.Api;
using WordGame.Construction.Change;
using WordGame.Construction.Verification;
using WordGame.UnitTests;

namespace WordGame.Construction.Tests;

// #region tests
public class DailyChallengeTests
{
    private static readonly DateOnly Day = new(2026, 11, 2);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 2, 23, 59, 0, TimeSpan.Zero));
    private readonly FakeUserRepository _users = new();
    private readonly DailyChallenge _challenge;

    public DailyChallengeTests()
    {
        _challenge = new DailyChallenge(Vocabulary.EnglishToGerman, new UserService(_users), _time);
    }

    [Fact]
    public void WordsFor_SameDay_SameWords() =>
        Assert.Equal(_challenge.WordsFor(Day), _challenge.WordsFor(Day));

    [Fact]
    public void WordsFor_NextDay_DifferentWords() =>
        Assert.NotEqual(_challenge.WordsFor(Day), _challenge.WordsFor(Day.AddDays(1)));

    [Fact]
    public void Complete_AfterMidnight_ChecksTheWordsThatWereShown()
    {
        var shownWords = _challenge.WordsFor(_challenge.Today);
        _time.Advance(TimeSpan.FromMinutes(2));

        var completed = _challenge.Complete("player@test.com", Day, Translate(shownWords));

        Assert.True(completed);
        Assert.Equal(DailyChallenge.Bonus, _users.FindByEmail("player@test.com")!.Experience);
    }

    // The old version accepted this: Zip stops at the shorter array, and All() of nothing is true
    [Fact]
    public void Complete_NoAnswers_AwardsNothing()
    {
        var completed = _challenge.Complete("player@test.com", Day, []);

        Assert.False(completed);
        Assert.Null(_users.FindByEmail("player@test.com"));
    }

    [Fact]
    public void Complete_WithoutPlayer_Throws() =>
        Assert.Throws<ArgumentException>(() => _challenge.Complete(" ", Day, []));

    private static string[] Translate(string[] words) =>
        [.. words.Select(word => WordBank.EnglishToGerman[word])];
}
// #endregion tests
