namespace WebStok.Domain.Entities;

public class OrganizationalUnit
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public OrganizationalUnit? Parent { get; set; }

    public bool IsActive { get; set; } = true;
}