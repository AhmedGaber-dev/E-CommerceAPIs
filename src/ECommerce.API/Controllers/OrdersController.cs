using Asp.Versioning;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Orders;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders([FromQuery] OrderListParameters parameters, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetOrdersQuery(parameters), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken));

    [HttpPost("checkout")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> Checkout(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new CreateOrderFromCartCommand(), cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateOrderStatusCommand(id, body.Status), cancellationToken);
        return NoContent();
    }

    public record UpdateOrderStatusBody(OrderStatus Status);
}
