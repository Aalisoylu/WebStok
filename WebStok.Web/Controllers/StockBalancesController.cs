using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStok.Business.Interfaces;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class StockBalancesController : Controller
{
    private readonly IStockBalanceService _service;

    public StockBalancesController(IStockBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var balances = await _service.ListAsync(cancellationToken);
        return View(balances);
    }
}