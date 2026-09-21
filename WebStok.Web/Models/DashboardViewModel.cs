using WebStok.Business.DTOs;
namespace WebStok.Web.Models;
public class DashboardViewModel
{
    public List<StockBalanceDto> Balances { get; set; } = new();
    public List<StockMovementListDto> Movements { get; set; } = new();
}
