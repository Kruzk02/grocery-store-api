using System.Security.Claims;

using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Services;

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

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await userService.CreateUser(dto);
        return Ok(new UserResponse(result));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await userService.Login(dto);
        return Ok(new TokenResponse(result));
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        var result = await userService.UpdateUser(User, dto);
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

            var serviceResult = await notificationService.FindByUserId(userId!);
            var json = System.Text.Json.JsonSerializer.Serialize(serviceResult);
            await Response.WriteAsync(json);
            await Response.Body.FlushAsync();

            await Task.Delay(3600000); // 1 Hour
        }

    }
}
