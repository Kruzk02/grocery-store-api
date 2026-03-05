using Application.Dtos.Request;
using Application.Interface;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("[controller]"), Authorize]
public class OrderItemController(IOrderItemService orderItemService) : ControllerBase
{
    [HttpPost,
     ProducesResponseType(typeof(OrderItem), 201),
     ProducesResponseType(400),
     ProducesResponseType(500),
    Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] OrderItemDto orderItemDto)
    {
        OrderItem result = await orderItemService.Create(orderItemDto);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}"),
     ProducesResponseType(typeof(OrderItem), 204),
     ProducesResponseType(400),
     ProducesResponseType(404),
     ProducesResponseType(500),
    Authorize(Roles = "Admin"),
    Authorize(Roles = "Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderItemDto orderItemDto)
    {
        OrderItem result = await orderItemService.Update(id, orderItemDto);
        return Ok(result);
    }

    [HttpGet("{id:int}"),
     ProducesResponseType(typeof(OrderItem), 200),
     ProducesResponseType(400),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> FindById(int id)
    {
        OrderItem result = await orderItemService.FindById(id);
        return Ok(result);
    }

    [HttpDelete("{id:int}"),
     ProducesResponseType(204),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await orderItemService.Delete(id);
        return result ? NoContent() : BadRequest();
    }
}
