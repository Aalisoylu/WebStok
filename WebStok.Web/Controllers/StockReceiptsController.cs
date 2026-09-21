using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class StockReceiptsController : Controller
{
    private readonly IStockReceiptService _service;

    public StockReceiptsController(IStockReceiptService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        return View(new StockReceiptPageViewModel
        {
            Data = await _service.GetPageDataAsync(cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] StockReceiptDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var operationId = await _service.ReceiveAsync(
                    input, cancellationToken);

                TempData["Success"] =
                    $"Mal kabul kaydedildi. İşlem: {operationId}";

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
            catch (SqliteException exception)
                when (exception.SqliteErrorCode is 5 or 6)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Veritabanı başka bir işlemi tamamlıyor. " +
                    "Biraz bekleyip aynı formu tekrar gönderiniz.");
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqliteException
                      { SqliteErrorCode: 5 or 6 })
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Veritabanı başka bir işlemi tamamlıyor. " +
                    "Biraz bekleyip aynı formu tekrar gönderiniz.");
            }
        }

        return View("Index", new StockReceiptPageViewModel
        {
            Input = input,
            Data = await _service.GetPageDataAsync(cancellationToken)
        });
    }
}