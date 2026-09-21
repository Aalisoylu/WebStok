using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebStok.Web.Controllers;


[Authorize(Policy = "ManageInventory")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await BuildPageAsync(
            new CreateCategoryDto(), cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] CreateCategoryDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _categoryService.CreateAsync(
                    input, cancellationToken);

                TempData["Success"] = "Kategori başarıyla eklendi.";

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
        }

        var model = await BuildPageAsync(input, cancellationToken);

        return View("Index", model);
    }

    private async Task<CategoryPageViewModel> BuildPageAsync(
        CreateCategoryDto input,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.ListAsync(
            cancellationToken);

        var lookup = categories.ToDictionary(x => x.Id);
        var paths = new Dictionary<int, string>();

        foreach (var category in categories)
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

            paths[category.Id] = string.Join(" / ", names);
        }

        return new CategoryPageViewModel
        {
            Input = input,
            Categories = categories
                .OrderBy(x => paths[x.Id])
                .ToList(),
            CategoryPaths = paths,
            ParentOptions = categories
                .Where(x => x.IsActive)
                .OrderBy(x => paths[x.Id])
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = paths[x.Id]
                })
                .ToList()
        };
    }
}