namespace ECommerce.Application.Features.Auth;

public record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc, UserSummaryDto User);

public record UserSummaryDto(Guid Id, string Email, string FirstName, string LastName, string Role);
