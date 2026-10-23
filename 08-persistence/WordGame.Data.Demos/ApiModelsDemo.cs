using System.Text.Json;

namespace WordGame.Data.Demos;

// What happens when entities are returned from and bound to web API endpoints.
// ASP.NET uses System.Text.Json with JsonSerializerOptions.Web, the same as below.
public class ApiModelsDemo(DatabaseFixture database)
{
    // #region cycle
    // GET /api/users/{email} returning the entity
    [Fact]
    public void ReturningEntity_FailsOnCycle()
    {
        using var db = database.CreateContext();

        var user = db.Users.Include(u => u.Games).Single(u => u.Email == "player1@test.com");

        // user.Games[0].User is the same user again, so the JSON would never end
        var error = Assert.Throws<JsonException>(() =>
            JsonSerializer.Serialize(user, JsonSerializerOptions.Web)
        );
        Report.WriteLine(error.Message);
    }

    // #endregion cycle

    // #region response
    // The response contains exactly what the client needs. A new column in the users table
    // (like a password hash) does not end up in the response by accident.
    public record UserResponse(string Email, int Experience, int GamesPlayed);

    [Fact]
    public void ReturningDto_SendsOnlyWhatTheClientNeeds()
    {
        using var db = database.CreateContext();

        var response = db
            .Users.Where(u => u.Email == "player1@test.com")
            .Select(u => new UserResponse(u.Email, u.Experience, u.Games.Count))
            .Single();

        Report.WriteLine(JsonSerializer.Serialize(response, JsonSerializerOptions.Web));
    }

    // #endregion response

    // #region over-posting
    // PUT /api/users/{id} meant for changing the email, with the body bound to the entity
    [Fact]
    public void BindingRequestToEntity_LetsClientChangeAnything()
    {
        var (id, email) = AddUser();
        var body = $$"""{ "id": {{id}}, "email": "{{email}}", "experience": 1000000 }""";
        var log = new SqlLog();
        using var db = database.CreateContext(log);

        var user = JsonSerializer.Deserialize<User>(body, JsonSerializerOptions.Web)!;
        db.Users.Update(user); // Marks every property as changed
        db.SaveChanges();

        Assert.Equal(1_000_000, database.ExperienceOf(email));
    }

    // #endregion over-posting

    // #region request-dto
    // The request type has only the fields the client is allowed to change
    public record ChangeEmailRequest(string Email);

    [Fact]
    public void BindingRequestToDto_ChangesOnlyAllowedFields()
    {
        var (id, email) = AddUser();
        var body = $$"""{ "id": {{id}}, "email": "new-{{email}}", "experience": 1000000 }""";
        using var db = database.CreateContext();

        var request = JsonSerializer.Deserialize<ChangeEmailRequest>(
            body,
            JsonSerializerOptions.Web
        )!;
        var user = db.Users.Single(u => u.Id == id);
        user.Email = request.Email;
        db.SaveChanges();

        Assert.Equal(0, database.ExperienceOf($"new-{email}"));
    }

    // #endregion request-dto

    private (int Id, string Email) AddUser()
    {
        var email = database.AddUser();
        using var db = database.CreateContext();
        return (db.Users.Single(u => u.Email == email).Id, email);
    }
}
