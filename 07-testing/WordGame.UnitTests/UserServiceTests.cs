using NSubstitute;
using WordGame.Api;

namespace WordGame.UnitTests;

public class UserServiceTests
{
    private readonly FakeUserRepository _repository = new();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(_repository);
    }

    [Fact]
    public void AddExperience_NewUser_StartsFromZero()
    {
        _userService.AddExperience("player@test.com", 10);

        Assert.Equal(10, _userService.GetUserByEmail("player@test.com")!.Experience);
    }

    [Fact]
    public void AddExperience_ExistingUser_AddsToExperience()
    {
        _userService.AddExperience("player@test.com", 10);

        _userService.AddExperience("player@test.com", 20);

        Assert.Equal(30, _userService.GetUserByEmail("player@test.com")!.Experience);
    }

    [Fact]
    public void AddExperience_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _userService.AddExperience("player@test.com", -10));
    }

    [Fact]
    public void GetUserByEmail_UnknownUser_ReturnsNull()
    {
        Assert.Null(_userService.GetUserByEmail("nobody@test.com"));
    }

    // #region mock
    // Mock: NSubstitute generates the implementation, and the test checks how it was called.
    // This verifies the implementation (exactly one save), not the behavior - if UserService
    // started saving in batches, this test would fail although users still get their experience.
    [Fact]
    public void AddExperience_SavesUserExactlyOnce()
    {
        var repository = Substitute.For<IUserRepository>();
        repository.FindByEmail("player@test.com").Returns(new User { Email = "player@test.com", Experience = 5 });
        var userService = new UserService(repository);

        userService.AddExperience("player@test.com", 10);

        repository.Received(1).AddOrUpdate(Arg.Is<User>(u => u.Experience == 15));
    }
    // #endregion mock
}
