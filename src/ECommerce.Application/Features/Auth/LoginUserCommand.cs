using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Features.Auth;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public LoginUserCommandHandler(
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Common.Exceptions.AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(string.Empty, "Invalid email or password.")
            });

        var token = _jwtTokenService.CreateToken(user, cancellationToken);
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var summary = new UserSummaryDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.Name);
        return new AuthResponseDto(token, expires, summary);
    }
}
