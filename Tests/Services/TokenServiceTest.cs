using System.IdentityModel.Tokens.Jwt;

using Application.Services;
using Application.Settings;

using Domain.Entity;

using Microsoft.Extensions.Options;

namespace Tests.Services;

[TestFixture]
public class TokenServiceTest
{
    [Test]
    [TestCaseSource(nameof(CreateApplicationUser))]
    public async Task CreateToken_ShouldReturn_ValidJwtToken(User user)
    {
        var jwtSettings = new JwtSettings
        {
            Key = "supersecretkey12345678901234567890",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };


        var options = Options.Create(jwtSettings);

        var tokenService = new TokenService(options);

        var token = await tokenService.CreateToken(user, null);

        Assert.That(string.IsNullOrWhiteSpace(token), Is.False);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Id, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value));
            Assert.That(user.Username, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value));
            Assert.That(user.Email, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value));
        }
    }

    private static IEnumerable<User> CreateApplicationUser()
    {
        yield return new User
        {
            Id = "123",
            Username = "testuser",
            Email = "test@example.com",
            Password = "123"
        };

        yield return new User
        {
            Id = "456",
            Username = "testuser123",
            Email = "test@example.com",
            Password = "456",
        };

        yield return new User
        {
            Id = "789",
            Username = "testuser456",
            Email = "test@example.com",
            Password = "789"
        };
    }
}
