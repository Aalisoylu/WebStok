using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class ProductsController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public ProductsController(
        IProductService products,
        ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        return View(await BuildPageAsync(
            new CreateProductDto(), cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] CreateProductDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _products.CreateAsync(input, cancellationToken);

                TempData["Success"] = "Ürün başarıyla eklendi.";

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    ModelState.AddModelError(
                        $"Input.{error.PropertyName}",
                        error.ErrorMessage);
                }
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqliteException
                      { SqliteExtendedErrorCode: 2067 })
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ürün kodu veya barkod zaten kullanılıyor.");
            }
        }

        return View("Index", await BuildPageAsync(
            input, cancellationToken));
    }

    private async Task<ProductPageViewModel> BuildPageAsync(
        CreateProductDto input,
        CancellationToken cancellationToken)
    {
        var categories = await _categories.ListAsync(cancellationToken);
        var lookup = categories.ToDictionary(x => x.Id);
        var options = new List<SelectListItem>();

        foreach (var category in categories.Where(x => x.IsActive))
        {
            var names = new Stack<string>();
            var visited = new HashSet<int>();
            int? currentId = category.Id;

            while (currentId.HasValue &&
                   lookup.TryGetValue(currentId.Value, out var current))
            {
                if (!visited.Add(current.Id))
                {
                    names.Push("[Geçersiz kategori bağlantısı]");
                    break;
                }

                names.Push(current.Name);
                currentId = current.ParentId;
            }

            options.Add(new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = string.Join(" / ", names)
            });
        }

        return new ProductPageViewModel
        {
            Input = input,
            Products = await _products.ListAsync(cancellationToken),
            CategoryOptions = options.OrderBy(x => x.Text).ToList()
        };
    }
}