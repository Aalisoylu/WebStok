using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStok.Business.Interfaces;
using WebStok.Web.Models;
namespace WebStok.Web.Controllers;
public class HomeController : Controller
{
    private readonly IStockBalanceService _balances;
    private readonly IStockMovementService _movements;
    public HomeController(IStockBalanceService balances, IStockMovementService movements)
    { _balances = balances; _movements = movements; }
    [Authorize(Policy = "ManageInventory")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel { Balances = await _balances.ListAsync(cancellationToken) };
        model.Movements = await _movements.ListAsync(cancellationToken);
        return View(model);
    }
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
