using System.Linq.Expressions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Features.Auth;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ECommerce.Application.Tests;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Persists_New_User_When_Email_Is_Unique()
    {
        var userRepo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IGenericRepository<Role>>();
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.Users).Returns(userRepo.Object);
        uow.Setup(x => x.Roles).Returns(roleRepo.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var userRole = new Role { Id = Guid.NewGuid(), Name = Roles.User, CreatedAtUtc = DateTime.UtcNow };
        roleRepo
            .Setup(x => x.ListAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { userRole });

        userRepo.Setup(x => x.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");

        var handler = new RegisterUserCommandHandler(uow.Object, hasher.Object);
        var id = await handler.Handle(
            new RegisterUserCommand("new@example.com", "Password1!", "Test", "User"),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        userRepo.Verify(
            x => x.AddAsync(It.Is<User>(u => u.Email == "new@example.com" && u.RoleId == userRole.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_When_Email_Already_Registered()
    {
        var userRepo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IGenericRepository<Role>>();
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.Users).Returns(userRepo.Object);
        uow.Setup(x => x.Roles).Returns(roleRepo.Object);

        userRepo.Setup(x => x.GetByEmailAsync("dup@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "dup@example.com" });

        var hasher = new Mock<IPasswordHasher>();
        var handler = new RegisterUserCommandHandler(uow.Object, hasher.Object);

        await Assert.ThrowsAsync<Common.Exceptions.AppValidationException>(() =>
            handler.Handle(new RegisterUserCommand("dup@example.com", "Password1!", "A", "B"), CancellationToken.None));
    }
}
