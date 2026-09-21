using Microsoft.AspNetCore.Mvc.Rendering;
using WebStok.Business.DTOs;

namespace WebStok.Web.Models;

public class CategoryPageViewModel
{
    public CreateCategoryDto Input { get; set; } = new();

    public List<CategoryListDto> Categories { get; set; } = new();

    public List<SelectListItem> ParentOptions { get; set; } = new();

    public Dictionary<int, string> CategoryPaths { get; set; } = new();
}