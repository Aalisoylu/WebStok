using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class StockTransfersController : Controller
{
    private readonly IStockTransferService _transferService;
    private readonly IStockReceiptService _receiptService;
    private readonly IStockBalanceService _balanceService;

    public StockTransfersController(
        IStockTransferService transferService,
        IStockReceiptService receiptService,
        IStockBalanceService balanceService)
    {
        _transferService = transferService;
        _receiptService = receiptService;
        _balanceService = balanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        return View(await BuildPageAsync(
            new StockTransferDto(), cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] StockTransferDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var operationId = await _transferService.TransferAsync(
                    input, cancellationToken);

                TempData["Success"] =
                    $"Transfer kaydedildi. İşlem: {operationId}";

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

        return View("Index", await BuildPageAsync(
            input, cancellationToken));
    }

    private async Task<StockTransferPageViewModel> BuildPageAsync(
        StockTransferDto input,
        CancellationToken cancellationToken)
    {
        var options = await _receiptService.GetPageDataAsync(
            cancellationToken);

        return new StockTransferPageViewModel
        {
            Input = input,

            ProductOptions = options.Products
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Label
                })
                .ToList(),

            WarehouseOptions = options.Warehouses
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Label
                })
                .ToList(),

            Balances = await _balanceService.ListAsync(
                cancellationToken)
        };
    }
}