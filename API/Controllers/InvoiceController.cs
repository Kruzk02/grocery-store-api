using Application.Dtos.Request;
using Application.Services;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("[controller]"), Authorize]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpPost,
     ProducesResponseType(typeof(Invoice), 201),
     ProducesResponseType(400),
     ProducesResponseType(500),
     Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] InvoiceDto invoiceDto)
    {
        var result = await invoiceService.Create(invoiceDto);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}"),
     ProducesResponseType(typeof(Invoice), 200),
     ProducesResponseType(404),
     ProducesResponseType(500)]
    public async Task<IActionResult> FindById(int id)
    {
        var result = await invoiceService.FindById(id);
        return Ok(result);
    }
}
