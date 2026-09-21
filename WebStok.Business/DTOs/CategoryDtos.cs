namespace WebStok.Business.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}

public class CategoryListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public bool IsActive { get; set; }
}