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
public class StockIssuesController : Controller
{
    private readonly IStockIssueService _issueService;
    private readonly IStockReceiptService _receiptService;
    private readonly IStockBalanceService _balanceService;

    public StockIssuesController(
        IStockIssueService issueService,
        IStockReceiptService receiptService,
        IStockBalanceService balanceService)
    {
        _issueService = issueService;
        _receiptService = receiptService;
        _balanceService = balanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        return View(await BuildPageAsync(
            new StockIssueDto(), cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "Input")] StockIssueDto input,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var operationId = await _issueService.IssueAsync(
                    input, cancellationToken);

                TempData["Success"] =
                    $"Stok çıkışı kaydedildi. İşlem: {operationId}";

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

    private async Task<StockIssuePageViewModel> BuildPageAsync(
        StockIssueDto input,
        CancellationToken cancellationToken)
    {
        var options = await _receiptService.GetPageDataAsync(
            cancellationToken);

        return new StockIssuePageViewModel
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