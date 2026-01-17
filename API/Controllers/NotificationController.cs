using System.Security.Claims;

using Application.Services;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("[controller]"), Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(Notification), 204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        await notificationService.DeleteById(id);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Notification), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await notificationService.MarkAsRead(id);
        return Ok(result);
    }

    [HttpPut("all-as-read")]
    [ProducesResponseType(typeof(Notification), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return BadRequest();
        }
        var result = await notificationService.MarkAllAsRead(userId);
        return Ok(result);
    }
}
