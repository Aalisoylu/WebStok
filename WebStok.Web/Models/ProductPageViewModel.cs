using Microsoft.AspNetCore.Mvc.Rendering;
using WebStok.Business.DTOs;

namespace WebStok.Web.Models;

public class ProductPageViewModel
{
    public CreateProductDto Input { get; set; } = new();

    public List<ProductListDto> Products { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();
}