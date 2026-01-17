using API.Documents;
using Application.Dtos;
using Application.Dtos.Request;
using Application.Services;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace API.Controllers;

[ApiController, Route("[controller]"), Authorize]
public class OrderController(IOrderService orderService, IOrderItemService itemService, IInvoiceService invoiceService) : ControllerBase
{

    [HttpPost,
     ProducesResponseType(typeof(Order), 201),
     ProducesResponseType(400),
     ProducesResponseType(500),
     Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] OrderDto orderDto)
    {
        var result = await orderService.Create(orderDto);

        return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}"),
     ProducesResponseType(200),
     ProducesResponseType(400),
     ProducesResponseType(404),
     ProducesResponseType(500),
     Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderDto orderDto)
    {
        var result = await orderService.Update(id, orderDto);
        return Ok(result);
    }

    [HttpGet("{id:int}"),
     ProducesResponseType(typeof(Order), 200),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> FindById(int id)
    {
        var result = await orderService.FindById(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/ordersItem"),
     ProducesResponseType(typeof(Order), 200),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> FindOrderItemById(int id)
    {
        var result = await itemService.FindByOrderId(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice"),
     ProducesResponseType(typeof(Invoice), 200),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> FindInvoiceById(int id)
    {
        var result = await invoiceService.FindByOrderId(id);
        var document = new InvoiceDocument(result);
        var pdf = document.GeneratePdf();
        return File(pdf, "application/pdf");
    }

    [HttpDelete("{id:int}"),
     ProducesResponseType(typeof(Order), 204),
     ProducesResponseType(404),
     ProducesResponseType(500),
     Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await orderService.Delete(id);
        return result ? NoContent() : BadRequest();
    }
}