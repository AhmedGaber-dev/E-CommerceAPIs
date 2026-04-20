using Asp.Versioning;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Paged user directory with search, role filter, and sorting.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserListParameters parameters, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUsersQuery(parameters), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUserByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateUserCommand(id, body.FirstName, body.LastName, body.RoleName), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    public record UpdateUserBody(string FirstName, string LastName, string? RoleName);
}
