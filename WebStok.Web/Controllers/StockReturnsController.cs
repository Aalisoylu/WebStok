using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
namespace WebStok.Web.Controllers;
[Authorize(Policy = "ManageInventory")]
public class StockReturnsController : Controller
{
    private readonly IStockReturnService _service;
    public StockReturnsController(IStockReturnService service) => _service = service;
    [HttpGet]
    public IActionResult Index(long? movementId) => View(new StockReturnDto { OriginalMovementId = movementId ?? 0 });
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockReturnDto input, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _service.ReturnAsync(input, cancellationToken);
                TempData["Success"] = "İade kaydedildi. Stok miktarı güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            catch (SqliteException exception) when (exception.SqliteErrorCode is 5 or 6)
            { ModelState.AddModelError(string.Empty, "Veritabanı meşgul. Aynı formu biraz sonra tekrar gönderiniz."); }
            catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 5 or 6 })
            { ModelState.AddModelError(string.Empty, "Veritabanı meşgul. Aynı formu biraz sonra tekrar gönderiniz."); }
        }
        return View("Index", input);
    }
}
