using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStok.Business.Interfaces;

namespace WebStok.Web.Controllers;

[Authorize(Policy = "ManageInventory")]
public class StockMovementsController : Controller
{
    private readonly IStockMovementService _service;

    public StockMovementsController(IStockMovementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var movements = await _service.ListAsync(cancellationToken);
        return View(movements);
    }
}