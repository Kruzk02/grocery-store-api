using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Application.Services.impl;
using Application.Settings;

using Domain.Entity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Moq;

namespace Tests.Services;

[TestFixture]
public class TokenServiceTest
{
    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Test]
    [TestCaseSource(nameof(CreateApplicationUser))]
    public async Task CreateToken_ShouldReturn_ValidJwtToken(ApplicationUser user)
    {
        var mockUserManager = MockUserManager<ApplicationUser>();

        mockUserManager
            .Setup(m => m.GetClaimsAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<Claim>());

        mockUserManager
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        var userManager = mockUserManager.Object;

        var jwtSettings = new JwtSettings
        {
            Key = "supersecretkey12345678901234567890",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };

        var options = Options.Create(jwtSettings);

        var tokenService = new TokenService(options);

        Debug.Assert(mockUserManager != null, nameof(mockUserManager) + " != null");
        var token = await tokenService.CreateToken(user, userManager);

        Assert.That(string.IsNullOrWhiteSpace(token), Is.False);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Id, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value));
            Assert.That(user.UserName, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value));
            Assert.That(user.Email, Is.EqualTo(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value));
        }
    }

    private static IEnumerable<ApplicationUser> CreateApplicationUser()
    {
        yield return new ApplicationUser
        {
            Id = "123",
            UserName = "testuser",
            Email = "test@example.com"
        };

        yield return new ApplicationUser
        {
            Id = "456",
            UserName = "testuser123",
            Email = "test@example.com"
        };

        yield return new ApplicationUser
        {
            Id = "789",
            UserName = "testuser456",
            Email = "test@example.com"
        };
    }
}
