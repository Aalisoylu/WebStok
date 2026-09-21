namespace WebStok.Domain.Entities;

public enum UserRole
{
    SuperAdmin = 1,
    ITAdmin = 2,
    WarehouseSupervisor = 3,
    UnitManager = 4,
    Personnel = 5,
    HumanResources = 6
}

public class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Personnel;

    public int? OrganizationalUnitId { get; set; }

    public OrganizationalUnit? OrganizationalUnit { get; set; }

    public int? ManagerId { get; set; }

    public AppUser? Manager { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}