using System.Security.Claims;

using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interface;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
        IUserService userService,
        INotificationService notificationService
    ) : ControllerBase
{

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        _ = await userService.CreateUser(dto);
        return Ok(new UserResponse("Success create user"));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        AuthResponse authResponse = await userService.Login(dto);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = authResponse.RefreshTokenExpiry
        };

        Response.Cookies.Append("refreshToken", authResponse.RefreshToken, cookieOptions);

        return Ok(new TokenResponse(authResponse.AccessToken));
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null)
        {
            return Unauthorized("Inva;od refresh token");
        }
        AuthResponse auth = await userService.RefreshToken(refreshToken);

        Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = auth.RefreshTokenExpiry
        });

        return Ok(new TokenResponse(auth.AccessToken));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] string usernameOrEmail)
    {
        User result = await userService.GetUser(usernameOrEmail);
        return Ok(new { result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var result = await userService.UpdateUser(id, dto);
        return Ok(new UserResponse(result));
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
    {
        var result = await userService.DeleteUser(request.Id);
        return result ? NoContent() : BadRequest();
    }

    [Authorize]
    [HttpGet("notifications")]
    public async Task FindNotificationByUserId()
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        while (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Notification> serviceResult = await notificationService.FindByUserId(userId!);
            var json = System.Text.Json.JsonSerializer.Serialize(serviceResult);
            await Response.WriteAsync(json);
            await Response.Body.FlushAsync();

            await Task.Delay(3600000); // 1 Hour
        }

    }
}
