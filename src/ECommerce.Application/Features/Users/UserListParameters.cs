using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Features.Users;

public class UserListParameters : SortableFilterRequest
{
    public string? Role { get; set; }
}
