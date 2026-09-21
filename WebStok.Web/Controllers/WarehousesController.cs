using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class WarehousesController : Controller
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        return View(new WarehousePageViewModel
        {
            Warehouses = await _warehouseService.ListAsync(
                cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] CreateWarehouseDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _warehouseService.CreateAsync(
                    input, cancellationToken);

                TempData["Success"] = "Depo başarıyla eklendi.";

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
                    "Input.Code",
                    "Bu depo kodu zaten kullanılıyor.");
            }
        }

        return View("Index", new WarehousePageViewModel
        {
            Input = input,
            Warehouses = await _warehouseService.ListAsync(
                cancellationToken)
        });
    }
}