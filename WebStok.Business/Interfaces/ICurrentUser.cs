using WebStok.Domain.Entities;

namespace WebStok.Business.Interfaces;

public interface ICurrentUser
{
    int? UserId { get; }

    UserRole? Role { get; }
}